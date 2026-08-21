using UnityEngine;
using System.Collections;
using System;
using System.Net;
// UFE Legacy API Compatibility aliases (see "UFE Addons/Compatibility/UFELegacyAPICompat.cs")
using Network = NetworkCompat;

public class NetworkGameScreen : UFEScreen{
	public virtual void GoToMainMenu(){
		UFE.StartMainMenuScreen();
	}

	public virtual void GoToHostGameScreen(){
		UFE.StartHostGameScreen();
	}

	public virtual void GoToJoinGameScreen(){
		UFE.StartJoinGameScreen();
	}

    public virtual string GetIPv6() {
        string hostName = System.Net.Dns.GetHostName();
        IPHostEntry ipHostEntry = System.Net.Dns.GetHostEntry(hostName);
        IPAddress[] ipAddresses = ipHostEntry.AddressList;
		
        return ipAddresses[ipAddresses.Length - 1].ToString();
    }

	public virtual string GetIP() {
        return Network.player.ipAddress.ToString();
	}
}
