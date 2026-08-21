using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace UFENetcode
{
	/// <summary>
	/// UFE 接口（UFEInterface）：标记接口——实现该接口的组件可被帧同步状态追踪器（RecordVar）记录/恢复状态。
	/// </summary>
    public interface UFEInterface { }

	/// <summary>
	/// UFE 行为基类（UFEBehaviour）。
	/// <para>用途：帧同步行为组件的基类，提供由帧同步系统驱动的固定帧更新方法 UFEFixedUpdate。</para>
	/// </summary>
    public class UFEBehaviour: MonoBehaviour {
		/// <summary>
		/// 帧同步固定帧更新（由 MrFusion/FluxCapacitor 调用）。
		/// </summary>
        public virtual void UFEFixedUpdate() { }
    }

	/// <summary>
	/// 状态追踪标记属性（RecordVar）。
	/// <para>用途：标记需要被帧同步状态保存/恢复的字段或属性（在字段上添加 [RecordVar]），</para>
	/// <para>配合静态方法 SaveStateTrackers/LoadStateTrackers 实现基于反射的任意对象状态快照与恢复，</para>
	/// <para>支持 UFEInterface 对象递归、List、Dictionary、Array 等容器类型。</para>
	/// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class RecordVar : Attribute
    {
		/// <summary>
		/// 构造函数。
		/// </summary>
        public RecordVar() { }

		/// <summary>
		/// 保存对象上所有 [RecordVar] 标记成员的状态到字典（递归处理嵌套 UFE 接口与容器）。
		/// </summary>
		/// <param name="sourceObj">源对象（UFE 接口）。</param>
		/// <param name="targetDictionary">目标状态字典（MemberInfo→值）。</param>
		/// <returns>填充后的状态字典；源对象为 null 时返回 null。</returns>
        public static Dictionary<System.Reflection.MemberInfo, System.Object> SaveStateTrackers(UFEInterface sourceObj, Dictionary<System.Reflection.MemberInfo, System.Object> targetDictionary)
        {
            if (sourceObj == null) return null;
            
            MemberInfo[] members = sourceObj.GetType().GetMembers();
            foreach (var prop in members)
            {
                //System.Reflection.FieldInfo fieldInfo = sourceObj.GetType().GetField(prop.Name);
                //System.Reflection.PropertyInfo propertyInfo = sourceObj.GetType().GetProperty(prop.Name);
                System.Reflection.FieldInfo fieldInfo = prop as System.Reflection.FieldInfo; // Perfomance improvement
                System.Reflection.PropertyInfo propertyInfo = prop as System.Reflection.PropertyInfo; // Perfomance improvement

                //RecordVar[] recordAttr = (RecordVar[])prop.GetCustomAttributes(typeof(RecordVar), false);
                bool isRecord = (fieldInfo != null || propertyInfo != null) && prop.IsDefined(typeof(RecordVar), false); //Perfomance improvement
                
                if (isRecord) {
                    System.Object objValue = null;
                    Type objType = sourceObj.GetType();

                    if (fieldInfo != null) {
                        objValue = fieldInfo.GetValue(sourceObj);
                        objType = fieldInfo.FieldType;
                    } else if (propertyInfo != null) {
                        objValue = propertyInfo.GetValue(sourceObj, null);
                        objType = propertyInfo.PropertyType;
                    }

                    if (objValue is UFEInterface || (objType != null && IsUFEInterface(objType))) {
                        // Object is UFE Interface
                        Dictionary<System.Reflection.MemberInfo, System.Object> recursiveDictionary = new Dictionary<System.Reflection.MemberInfo, System.Object>();

                        // Save the object reference itself
                        recursiveDictionary.Add(prop, objValue);

                        objValue = SaveStateTrackers(objValue as UFEInterface, recursiveDictionary);

                    } else if (objValue != null && objType != null && objType.IsGenericType && (objType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)) || objType is IList)) {
                        // Object is List Type
                        objValue = SaveListTracker(objValue as IList, prop);
                        
                    } else if (objValue != null && objType != null && objType.IsGenericType && objType.GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>))) {
                        // Object is Dictionary Type
                        Type type1 = objValue.GetType().GetGenericArguments()[0]; // Key
                        Type type2 = objValue.GetType().GetGenericArguments()[1]; // Value
                        objValue = SaveDictionaryTracker(objValue as IDictionary, type1, type2, prop);

                    } else if (objValue != null && objType != null && objType.IsArray) {
                        // Object is Array Type
                        objValue = SaveArrayTracker(objValue, prop);
                    }
                    
                    if (!targetDictionary.ContainsKey(prop)) {
                        targetDictionary.Add(prop, objValue);
                    } else {
                        targetDictionary[prop] = objValue;
                    }
                }
            }

            return targetDictionary;
        }
        
		/// <summary>
		/// 从状态字典恢复对象上所有 [RecordVar] 标记成员的状态（递归恢复嵌套 UFE 接口与容器）。
		/// </summary>
		/// <param name="targetObj">目标对象（UFE 接口）。</param>
		/// <param name="sourceDictionary">源状态字典（MemberInfo→值）。</param>
		/// <returns>恢复后的目标对象；目标为 null 时返回 null。</returns>
        public static UFEInterface LoadStateTrackers(UFEInterface targetObj, Dictionary<System.Reflection.MemberInfo, System.Object> sourceDictionary)
        {
            if (targetObj == null) return null;

            MemberInfo[] members = targetObj.GetType().GetMembers();
            foreach (var prop in members)
            {
                System.Reflection.FieldInfo fieldInfo = prop as System.Reflection.FieldInfo;
                System.Reflection.PropertyInfo propertyInfo = prop as System.Reflection.PropertyInfo;
                
                bool isRecord = (fieldInfo != null || propertyInfo != null) && prop.IsDefined(typeof(RecordVar), false);

                if (isRecord) {
                    var objValue = sourceDictionary[prop];
                    Type objType = null;

                    if (fieldInfo != null) objType = fieldInfo.FieldType;
                    if (propertyInfo != null) objType = propertyInfo.PropertyType;

                    if (objValue != null && objValue is Dictionary<System.Reflection.MemberInfo, System.Object>) {
                        // Object is Recursive Dictionary
                        var recursiveObject = (objValue as Dictionary<System.Reflection.MemberInfo, System.Object>)[prop];
                        if (fieldInfo != null) fieldInfo.SetValue(targetObj, recursiveObject);
                        if (propertyInfo != null) propertyInfo.SetValue(targetObj, recursiveObject, null);

                        objValue = LoadStateTrackers(recursiveObject as UFEInterface, objValue as Dictionary<System.Reflection.MemberInfo, System.Object>);

                    } else if (objValue != null && objType != null && (objType.IsGenericType && objType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)) || objType is IList)) {
                        // Object is List Type
                        objValue = LoadListTracker(objValue as IList, objType, prop);
                        
                    } else if (objValue != null && objType != null && objType.IsGenericType && objType.GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>))) {
                        // Object is Dictionary Type
                        Type type1 = objValue.GetType().GetGenericArguments()[0]; // Key
                        Type type2 = objValue.GetType().GetGenericArguments()[1]; // Value
                        objValue = LoadDictionaryTracker(objValue as IDictionary, type1, type2, prop);
                    
                    } else if (objValue != null && objType != null && objType.IsArray) {
                        // Object is Array Tracker Type
                        objValue = LoadArrayTracker(objValue as Dictionary<System.Reflection.MemberInfo, System.Object>[], objType.GetElementType(), prop);
                    }
                    
                    if (fieldInfo != null) fieldInfo.SetValue(targetObj, objValue);
                    if (propertyInfo != null) propertyInfo.SetValue(targetObj, objValue, null);
                }
            }
            return targetObj;
        }

		/// <summary>
		/// 保存列表状态（列表元素为 UFE 接口时递归保存）。
		/// </summary>
		/// <param name="source">源列表。</param>
		/// <param name="memberInfo">成员信息。</param>
		/// <returns>保存后的值列表。</returns>
        public static IList SaveListTracker(IList source, MemberInfo memberInfo)
        {
            List<System.Object> newList = new List<System.Object>();
            foreach (var entry in source as IList)
            {
                var newEntry = entry;
                if (entry is UFEInterface || IsUFEInterface(entry.GetType()))
                {
                    Dictionary<System.Reflection.MemberInfo, System.Object> recursiveDictionary = new Dictionary<System.Reflection.MemberInfo, System.Object>();
                    recursiveDictionary.Add(memberInfo, newEntry);

                    newEntry = SaveStateTrackers(entry as UFEInterface, recursiveDictionary);
                }
                newList.Add(newEntry);
            }

            return newList;
        }

		/// <summary>
		/// 保存字典状态（值为 UFE 接口时递归保存）。
		/// </summary>
		/// <param name="source">源字典。</param>
		/// <param name="key">键类型。</param>
		/// <param name="value">值类型。</param>
		/// <param name="memberInfo">成员信息。</param>
		/// <returns>保存后的值字典。</returns>
        public static IDictionary SaveDictionaryTracker(IDictionary source, Type key, Type value, MemberInfo memberInfo)
        {
            Type dictType = typeof(Dictionary<,>).MakeGenericType(key, value);
            IDictionary newDictionary = Activator.CreateInstance(dictType) as IDictionary;
            foreach (DictionaryEntry entry in source)
            {
                var newEntry = entry.Value;
                if (entry.Value is UFEInterface || IsUFEInterface(entry.Value.GetType()))
                {
                    Dictionary<System.Reflection.MemberInfo, System.Object> recursiveDictionary = new Dictionary<System.Reflection.MemberInfo, System.Object>();
                    recursiveDictionary.Add(memberInfo, newEntry);

                    newEntry = SaveStateTrackers(entry.Value as UFEInterface, recursiveDictionary);
                }
                newDictionary.Add(entry.Key, newEntry);
            }

            return newDictionary;
        }

		/// <summary>
		/// 保存数组状态（元素为 UFE 接口时递归保存）。
		/// </summary>
		/// <param name="source">源数组。</param>
		/// <param name="memberInfo">成员信息。</param>
		/// <returns>保存后的状态字典数组。</returns>
        public static Dictionary<System.Reflection.MemberInfo, System.Object>[] SaveArrayTracker(System.Object source, MemberInfo memberInfo)
        {
            Dictionary<System.Reflection.MemberInfo, System.Object>[] newArray = new Dictionary<System.Reflection.MemberInfo, System.Object>[(source as Array).Length];
            int i = 0;
            foreach (var entry in source as Array)
            {
                Dictionary<System.Reflection.MemberInfo, System.Object> newEntry = new Dictionary<System.Reflection.MemberInfo, System.Object>();
                if (entry != null && (entry is UFEInterface || IsUFEInterface(entry.GetType())))
                {
                    Dictionary<System.Reflection.MemberInfo, System.Object> recursiveDictionary = new Dictionary<System.Reflection.MemberInfo, System.Object>();
                    recursiveDictionary.Add(memberInfo, entry);

                    newEntry = SaveStateTrackers(entry as UFEInterface, recursiveDictionary);
                }
                else
                {
                    newEntry.Add(memberInfo, entry);
                }
                newArray[i] = newEntry;
                i++;
            }

            return newArray;
        }
        
		/// <summary>
		/// 恢复列表状态（元素为递归字典时递归恢复）。
		/// </summary>
		/// <param name="source">源值列表。</param>
		/// <param name="T">列表类型。</param>
		/// <param name="memberInfo">成员信息。</param>
		/// <returns>恢复后的列表。</returns>
        public static IList LoadListTracker(IList source, Type T, MemberInfo memberInfo)
        {
            IList newList = Activator.CreateInstance(T) as IList;
            foreach (var entry in source as IList)
            {
                var newEntry = entry;
                if (entry is Dictionary<System.Reflection.MemberInfo, System.Object>)
                {
                    var recursiveObject = (entry as Dictionary<System.Reflection.MemberInfo, System.Object>)[memberInfo];
                    newEntry = LoadStateTrackers(recursiveObject as UFEInterface, entry as Dictionary<System.Reflection.MemberInfo, System.Object>);
                }
                (newList as IList).Add(newEntry);
            }

            return newList;
        }

		/// <summary>
		/// 恢复字典状态（值为递归字典时递归恢复）。
		/// </summary>
		/// <param name="source">源值字典。</param>
		/// <param name="key">键类型。</param>
		/// <param name="value">值类型。</param>
		/// <param name="memberInfo">成员信息。</param>
		/// <returns>恢复后的字典。</returns>
        public static IDictionary LoadDictionaryTracker(IDictionary source, Type key, Type value, MemberInfo memberInfo)
        {
            Type dictType = typeof(Dictionary<,>).MakeGenericType(key, value);
            IDictionary newDictionary = Activator.CreateInstance(dictType) as IDictionary;
            foreach (DictionaryEntry entry in source)
            {
                var newEntry = entry.Value;
                if (entry.Value is Dictionary<System.Reflection.MemberInfo, System.Object>)
                {
                    var recursiveObject = (entry.Value as Dictionary<System.Reflection.MemberInfo, System.Object>)[memberInfo];
                    newEntry = LoadStateTrackers(recursiveObject as UFEInterface, entry.Value as Dictionary<System.Reflection.MemberInfo, System.Object>);

                    newEntry = LoadStateTrackers(recursiveObject as UFEInterface, entry.Value as Dictionary<System.Reflection.MemberInfo, System.Object>);
                }
                newDictionary.Add(entry.Key, newEntry);
            }

            return newDictionary;
        }

		/// <summary>
		/// 恢复数组状态（元素为递归字典时递归恢复）。
		/// </summary>
		/// <param name="source">源状态字典数组。</param>
		/// <param name="elementType">元素类型。</param>
		/// <param name="memberInfo">成员信息。</param>
		/// <returns>恢复后的数组。</returns>
        public static object LoadArrayTracker(Dictionary<System.Reflection.MemberInfo, System.Object>[] source, Type elementType, MemberInfo memberInfo)
        {
            var newArray = Array.CreateInstance(elementType, (source as Array).Length);
            int i = 0;
            foreach (var entry in source)
            {
                if (entry is Dictionary<System.Reflection.MemberInfo, System.Object>)
                {
                    var recursiveObject = (entry as Dictionary<System.Reflection.MemberInfo, System.Object>)[memberInfo];
                    newArray.SetValue(LoadStateTrackers(recursiveObject as UFEInterface, entry as Dictionary<System.Reflection.MemberInfo, System.Object>), i);
                }
                else
                {
                    newArray.SetValue(entry, i);
                }
                i++;
            }

            return newArray;
        }

		/// <summary>
		/// 判断类型是否实现了 UFEInterface 接口。
		/// </summary>
		/// <param name="T">目标类型。</param>
		/// <returns>实现返回 true。</returns>
        public static bool IsUFEInterface(Type T)
        {
            Type[] interfaceList = T.GetInterfaces();
            foreach (Type t in interfaceList)
            {
                if (t == typeof(UFEInterface)) return true;
            }
            return false;
        }
    }
}
