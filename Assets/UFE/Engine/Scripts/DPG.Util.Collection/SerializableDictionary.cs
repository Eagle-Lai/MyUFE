using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DGP.Util.Collections{
	/// <summary>
	/// 可序列化字典（SerializableDictionary）。
	/// <para>用途：Unity 默认 Dictionary 无法被 Inspector 序列化，本类使用两个 List（键/值）实现 IDictionary 接口，</para>
	/// <para>使字典可以在 Unity 编辑器中显示与保存，并在运行时保持有序（按键二分搜索）。</para>
	/// <para>键值类型需可序列化（通过 CanBeSerialized 校验）。</para>
	/// </summary>
	[Serializable]
	public class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>{
		#region constants
		/// <summary>默认容量。</summary>
		private const int DefaultCapacity = 4;
		#endregion

		#region public instance constructors
		/// <summary>默认构造函数（默认容量，无初始数据）。</summary>
		public SerializableDictionary() : this(-1, null){}
		/// <summary>指定容量构造函数。</summary>
		/// <param name="capacity">初始容量。</param>
		public SerializableDictionary(int capacity) : this(capacity, null){}
		/// <summary>从现有字典复制数据的构造函数。</summary>
		/// <param name="dictionary">源字典。</param>
		public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : this(-1, dictionary){}
		#endregion

		#region protected instance constructors
		/// <summary>
		/// 受保护的完整构造函数：校验键值类型可序列化后初始化键/值列表。
		/// </summary>
		/// <param name="capacity">初始容量。</param>
		/// <param name="dictionary">源字典（可为 null）。</param>
		protected SerializableDictionary(int capacity, IDictionary<TKey, TValue> dictionary){
			if(!SerializableDictionary<TKey, TValue>.CanBeSerialized(typeof(TKey))){
				throw new InvalidOperationException("TKey can't be serialized.");
			}

			if(!SerializableDictionary<TKey, TValue>.CanBeSerialized(typeof(TValue))){
				throw new InvalidOperationException("TValue can't be serialized.");
			}

			if (dictionary != null){
				capacity = dictionary.Count;
			}else if (capacity < 0){
				capacity = DefaultCapacity;
			}

			this._keys = new List<TKey>(capacity);
			this._values = new List<TValue>(capacity);

			if (dictionary != null){
				foreach (KeyValuePair<TKey, TValue> item in dictionary){
					this.Add(item.Key, item.Value);
				}
			}
		}
		#endregion

		#region protected instance properties
		/// <summary>键列表（可被 Unity 序列化）。</summary>
		[SerializeField]
		protected List<TKey> _keys;

		/// <summary>值列表（可被 Unity 序列化）。</summary>
		[SerializeField]
		protected List<TValue> _values;
		#endregion

		#region IEnumerable interface implementation: methods
		/// <summary>
		/// 非泛型枚举器：迭代键值对，检测集合被修改时抛出异常。
		/// </summary>
		/// <returns>键值对枚举器。</returns>
		IEnumerator IEnumerable.GetEnumerator(){
			int count = this.Count;
			
			for (int i = 0; i < count; ++i){
				// Check if the dictionary has been modified since the enumerator was created
				if (count != this.Count){
					throw new InvalidOperationException("Collection was modified.");
				}
				
				yield return new KeyValuePair<TKey, TValue>(this._keys[i], this._values[i]);
			}
		}
		#endregion

		#region ICollection<KeyValuePair<TKey, TValue>> interface implementation: properties
		/// <summary>集合是否只读（恒为 false，可修改）。</summary>
		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly{
			get{return false;}
		}
		#endregion

		#region ICollection<KeyValuePair<TKey, TValue>> interface implementation: methods
		/// <summary>
		/// 添加键值对。
		/// </summary>
		/// <param name="item">键值对。</param>
		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item){
			this.Add(item.Key, item.Value);
		}
		
		/// <summary>
		/// 判断集合是否包含指定键值对。
		/// </summary>
		/// <param name="item">键值对。</param>
		/// <returns>包含返回 true。</returns>
		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item){
			TValue value;
			
			return	item.Key != null && 
					this.TryGetValue(item.Key, out value) &&
					(value == null && item.Value == null || value != null && value.Equals(item.Value));
		}

		/// <summary>
		/// 将键值对复制到指定数组（从 arrayIndex 开始）。
		/// </summary>
		/// <param name="array">目标数组。</param>
		/// <param name="arrayIndex">起始索引。</param>
		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex){
			if (array == null){
				throw new ArgumentNullException("array");
			}else if (arrayIndex < 0){
				throw new ArgumentOutOfRangeException("arrayIndex");
			}else if (this.Count > array.Length - arrayIndex){
				throw new ArgumentException();
			}

			for (int i = 0; i < this.Count; ++i){
				array[arrayIndex + i] = new KeyValuePair<TKey, TValue>(this._keys[i], this._values[i]);
			}
		}

		/// <summary>
		/// 移除指定键值对（按键匹配）。
		/// </summary>
		/// <param name="item">键值对。</param>
		/// <returns>移除成功返回 true。</returns>
		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item){
			if (item.Key != null){
				int index = this._keys.BinarySearch(item.Key);
				if (index >= 0){
					this._keys.RemoveAt(index);
					this._values.RemoveAt(index);
					return true;
				}
			}
			
			return false;
		}
		#endregion

		#region IDictionary<TKey, TValue> implementation: properties
		/// <summary>字典元素数量。</summary>
		public virtual int Count{
			get{return this._keys.Count;}
		}

		/// <summary>
		/// 索引器：按键获取/设置值（不存在时按有序位置插入）。
		/// </summary>
		/// <param name="key">键。</param>
		/// <returns>对应值。</returns>
		public virtual TValue this[TKey key]{
			get{
				if (key == null){
					throw new ArgumentNullException();
				}

				int index = this._keys.BinarySearch(key);
				if (index < 0){
					throw new KeyNotFoundException();
				}

				return this._values[index];
			}
			set{
				if (key == null){
					throw new ArgumentNullException();
				}
				
				int index = this._keys.BinarySearch(key);
				if (index < 0){
					this._keys.Insert(~index, key);
					this._values.Insert(~index, value);
				}else{
					this._values[index] = value;
				}
			}
		}

		/// <summary>键集合（副本）。</summary>
		public virtual ICollection<TKey> Keys{
			get{
				return new List<TKey>(this._keys);
			}
		}

		/// <summary>值集合（副本）。</summary>
		public virtual ICollection<TValue> Values{
			get{
				return new List<TValue>(this._values);
			}
		}
		#endregion

		#region IDictionary<TKey, TValue> interfaceimplementation: methods
		/// <summary>
		/// 添加键值对（键已存在时抛出 ArgumentException）。
		/// </summary>
		/// <param name="key">键。</param>
		/// <param name="value">值。</param>
		public virtual void Add(TKey key, TValue value){
			if (key == null){
				throw new ArgumentNullException();
			}

			int index = this._keys.BinarySearch(key);
			if (index >= 0){
				throw new ArgumentException();
			}

			this._keys.Insert(~index, key);
			this._values.Insert(~index, value);
		}

		/// <summary>
		/// 清空字典。
		/// </summary>
		public virtual void Clear(){
			this._keys.Clear();
			this._values.Clear();
		}

		/// <summary>
		/// 判断是否包含指定键。
		/// </summary>
		/// <param name="key">键。</param>
		/// <returns>包含返回 true。</returns>
		public virtual bool ContainsKey(TKey key){
			if (key == null){
				throw new ArgumentNullException();
			}

			return this._keys.BinarySearch(key) >= 0;
		}

		/// <summary>
		/// 泛型枚举器：迭代键值对。
		/// </summary>
		/// <returns>键值对枚举器。</returns>
		public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator(){
			int count = this.Count;
			
			for (int i = 0; i < count; ++i){
				// Check if the dictionary has been modified since the enumerator was created
				if (count != this.Count){
					throw new InvalidOperationException("Collection was modified.");
				}
				
				yield return new KeyValuePair<TKey, TValue>(this._keys[i], this._values[i]);
			}
		}

		/// <summary>
		/// 按键移除元素。
		/// </summary>
		/// <param name="key">键。</param>
		/// <returns>移除成功返回 true。</returns>
		public virtual bool Remove(TKey key){
			if (key == null){
				throw new ArgumentNullException();
			}

			int index = this._keys.BinarySearch(key);
			if (index < 0){
				return false;
			}

			this._keys.RemoveAt(index);
			this._values.RemoveAt(index);
			return true;
		}

		/// <summary>
		/// 尝试获取指定键的值。
		/// </summary>
		/// <param name="key">键。</param>
		/// <param name="value">输出值（未找到时为默认值）。</param>
		/// <returns>找到返回 true。</returns>
		public virtual bool TryGetValue(TKey key, out TValue value){
			if (key == null){
				throw new ArgumentNullException();
			}
			
			int index = this._keys.BinarySearch(key);
			if (index < 0){
				value = default(TValue);
				return false;
			}

			value = this._values[index];
			return true;
		}
		#endregion

		#region public class methods
		/// <summary>
		/// 判断类型是否可被 Unity 序列化（基元/可序列化/字符串/UnityEngine.Object）。
		/// </summary>
		/// <param name="type">目标类型。</param>
		/// <returns>可序列化返回 true。</returns>
		public static bool CanBeSerialized(Type type){
			return 
				type.IsPrimitive ||
				type.IsSerializable || 
				type == typeof(String) ||
				typeof(UnityEngine.Object).IsAssignableFrom(type);
				// TODO: Add Arrays and List of types that can be serialized
		}
		#endregion
	}
}
