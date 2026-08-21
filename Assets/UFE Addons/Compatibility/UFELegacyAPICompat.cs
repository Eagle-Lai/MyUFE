using UnityEngine;

// ============================================================================
// UFE Legacy API Compatibility Layer
// ----------------------------------------------------------------------------
// UFE 插件最初为旧版 Unity（4.x/5.x）编写，使用了以下已被 Unity 2018.2+
// 移除的旧 API：
//   - GUIText / GUITexture   （旧 GUI 系统，已改为 uGUI）
//   - Network / NetworkView / NetworkPlayer / NetworkConnectionError /
//     NetworkPeerType / NetworkMessageInfo / NetworkDisconnection /
//     NetworkStateSynchronization / RPCMode / RPC  （旧 UNET 网络系统）
//
// 本文件提供与原 API 兼容的自定义类型，使 UFE 代码能够在 Unity 2020.3+
// 中编译通过。所有类型均以 "Compat" 后缀命名，避免与 UnityEngine 残留
// 的过时类型冲突。业务文件通过 using 别名引用这些兼容类型。
// ============================================================================

// ----------------------------------------------------------------------------
// GUIText 兼容组件：在 OnGUI 中渲染文本，模拟旧 GUIText 的显示行为
// ----------------------------------------------------------------------------
[ExecuteInEditMode]
public class GUITextCompat : MonoBehaviour {
	public string text = "";
	public Vector2 pixelOffset = Vector2.zero;
	public TextAnchor anchor = TextAnchor.UpperLeft;
	public TextAlignment alignment = TextAlignment.Left;
	public Color color = Color.white;
	public bool richText = true;

	private GUIStyle cachedStyle;
	private GUIStyle Style {
		get {
			if (cachedStyle == null) {
				cachedStyle = new GUIStyle(GUI.skin.label);
			}
			return cachedStyle;
		}
	}

	private void OnGUI() {
		if (!this.enabled) return;

		GUIStyle style = this.Style;
		style.normal.textColor = this.color;
		style.richText = this.richText;
		style.alignment = this.GUITextAnchorToGUIStyleAnchor(this.anchor, this.alignment);

		float screenW = Screen.width;
		float screenH = Screen.height;

		Rect rect = new Rect(0f, 0f, screenW, screenH);
		switch (this.anchor) {
			case TextAnchor.UpperLeft:
			case TextAnchor.MiddleLeft:
			case TextAnchor.LowerLeft:
				rect.x = this.pixelOffset.x;
				rect.y = this.pixelOffset.y;
				break;
			case TextAnchor.UpperCenter:
			case TextAnchor.MiddleCenter:
			case TextAnchor.LowerCenter:
				rect.x = screenW * 0.5f + this.pixelOffset.x;
				rect.y = this.pixelOffset.y;
				break;
			case TextAnchor.UpperRight:
			case TextAnchor.MiddleRight:
			case TextAnchor.LowerRight:
				rect.x = screenW + this.pixelOffset.x;
				rect.y = this.pixelOffset.y;
				break;
		}

		GUI.Label(rect, this.text ?? "", style);
	}

	private TextAnchor GUITextAnchorToGUIStyleAnchor(TextAnchor anchor, TextAlignment alignment) {
		// 兼容类型遵循 GUIText 语义：alignment 决定水平/垂直对齐，
		// anchor 决定参照位置。这里简单映射为 GUI 可用锚点。
		switch (alignment) {
			case TextAlignment.Center:
				return TextAnchor.MiddleCenter;
			case TextAlignment.Right:
				return TextAnchor.UpperRight;
			default:
				return TextAnchor.UpperLeft;
		}
	}
}

// ----------------------------------------------------------------------------
// GUITexture 兼容组件：在 OnGUI 中绘制纹理，模拟旧 GUITexture 的显示行为
// ----------------------------------------------------------------------------
[ExecuteInEditMode]
public class GUITextureCompat : MonoBehaviour {
	public Texture texture;
	public Rect pixelInset = new Rect(0f, 0f, Screen.width, Screen.height);
	public Color color = Color.white;

	private void OnGUI() {
		if (!this.enabled || this.texture == null) return;

		Color oldColor = GUI.color;
		GUI.color = this.color;
		GUI.DrawTexture(this.pixelInset, this.texture);
		GUI.color = oldColor;
	}
}

// ----------------------------------------------------------------------------
// 旧网络系统兼容类型
// ----------------------------------------------------------------------------

// NetworkConnectionError 兼容枚举
public enum NetworkConnectionErrorCompat {
	NoError = 0,
	RSAPublicKeyMismatch = 1,
	ConnectionFailed = 2,
	TooManyConnectedPlayers = 3,
	ConnectionBanned = 4,
	AlreadyConnectedToServer = 5,
	AlreadyConnectedToAnotherServer = 6,
	AlreadyConnectedToSameTutorial = 7,
	NoInternetPermission = 8,
	IncorrectParameters = 9,
	LoginFailed = 10,
	CreateSocketOrThreadFailure = 11,
	DNSLookupFailed = 12,
	NoSuchTarget = 13,
	AlreadyDisconnected = 14,
	ProxyConnectFailure = 15,
	NATTargetNotInNATTraversal = 16,
	NATSourceIPEqualsTargetIP = 17,
	NATTargetNotConnected = 18,
	NATSourceConnectionNotFound = 19,
	NATTargetConnectionNotFound = 20,
	InternalDirectConnectFailed = 21,
	NATTargetAddressIsLocal = 22,
	NATPunchthroughFailed = 23,
	InternalConnectFailure = 24,
	ConnectionTimeout = 25,
	IncompatibleVersions = 26
}

// NetworkPeerType 兼容枚举
public enum NetworkPeerTypeCompat {
	Disconnected = 0,
	Server = 1,
	Client = 2,
	Connecting = 3
}

// NetworkDisconnection 兼容枚举
public enum NetworkDisconnectionCompat {
	LostConnection = 0,
	Disconnected = 1
}

// RPCMode 兼容枚举
public enum RPCModeCompat {
	Server = 0,
	Others = 1,
	All = 2,
	AllBuffered = 3,
	OthersBuffered = 4
}

// NetworkStateSynchronization 兼容枚举
public enum NetworkStateSynchronizationCompat {
	Off = 0,
	ReliableDeltaCompressed = 1,
	Unreliable = 2
}

// NetworkPlayer 兼容结构体
public struct NetworkPlayerCompat {
	public string ipAddress { get { return this._ipAddress; } }
	public int port { get { return this._port; } }

	private readonly string _ipAddress;
	private readonly int _port;

	public NetworkPlayerCompat(string ipAddress, int port) {
		this._ipAddress = ipAddress ?? string.Empty;
		this._port = port;
	}

	public override string ToString() {
		return string.IsNullOrEmpty(this._ipAddress) ? "0.0.0.0" : this._ipAddress;
	}
}

// NetworkMessageInfo 兼容结构体
public struct NetworkMessageInfoCompat {
	public NetworkPlayerCompat sender {
		get { return new NetworkPlayerCompat("127.0.0.1", 0); }
	}
}

// RPC 兼容特性（仅保留标记作用）
public class RPCCompat : System.Attribute {
}

// NetworkView 兼容组件（旧网络视图。由于 legacy 网络系统已移除，
// 这里仅作为占位组件，保证 AddComponent/GetComponent 调用可编译。）
public class NetworkViewCompat : MonoBehaviour {
	public NetworkStateSynchronizationCompat stateSynchronization = NetworkStateSynchronizationCompat.Off;
	public UnityEngine.Object observed;

	public void RPC(string name, RPCModeCompat mode, params object[] args) {
		// Legacy 网络已不可用，此调用为空操作
	}
}

// Network 兼容静态类（旧网络 API 入口。legacy 网络系统已移除，
// 所有方法均返回"失败/已断开"，保证代码可编译且行为安全。）
public static class NetworkCompat {
	public static NetworkPlayerCompat player {
		get { return new NetworkPlayerCompat("127.0.0.1", 0); }
	}

	public static NetworkPeerTypeCompat peerType {
		get { return NetworkPeerTypeCompat.Disconnected; }
	}

	public static float sendRate { get; set; }

	public static NetworkPlayerCompat[] connections {
		get { return new NetworkPlayerCompat[0]; }
	}

	public static NetworkConnectionErrorCompat InitializeServer(int connections, int listenPort, bool useNat) {
		Debug.LogWarning("[NetworkCompat] InitializeServer 被调用，但旧版网络系统已被 Unity 移除，操作失败。");
		return NetworkConnectionErrorCompat.ConnectionFailed;
	}

	public static NetworkConnectionErrorCompat Connect(string ip, int port) {
		Debug.LogWarning("[NetworkCompat] Connect 被调用，但旧版网络系统已被 Unity 移除，操作失败。");
		return NetworkConnectionErrorCompat.ConnectionFailed;
	}

	public static NetworkConnectionErrorCompat Connect(string ip, int port, string password) {
		return NetworkCompat.Connect(ip, port);
	}

	public static void Disconnect() {
		// 空操作
	}

	public static void Disconnect(int timeout) {
		// 空操作
	}

	public static int GetLastPing(NetworkPlayerCompat player) {
		return 0;
	}

	public static void RemoveRPCs(NetworkPlayerCompat player) {
		// 空操作
	}

	public static void DestroyPlayerObjects(NetworkPlayerCompat player) {
		// 空操作
	}
}

// ----------------------------------------------------------------------------
// NetworkIdentity 兼容组件（Unity HLAPI 已移除，仅作为占位组件保证编译通过）
// ----------------------------------------------------------------------------
public class NetworkIdentityCompat : MonoBehaviour {
	// 兼容旧版 HLAPI 的 connectionToClient 属性
	public NetworkConnectionCompat connectionToClient {
		get { return null; }
	}

	public bool isClient { get { return false; } }
	public bool isServer { get { return false; } }
	public bool hasAuthority { get { return false; } }
	public string netId { get { return "0"; } }
}

// NetworkConnection 兼容类型（HLAPI 已移除，占位）
public class NetworkConnectionCompat {
	public string address { get { return string.Empty; } }
	public int connectionId { get { return 0; } }
	public int hostId { get { return 0; } }
	public bool isReady { get { return false; } }
	public float lastMessageTime { get { return 0f; } }
}
