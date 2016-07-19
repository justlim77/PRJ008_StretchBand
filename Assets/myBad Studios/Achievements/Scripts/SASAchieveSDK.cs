using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

namespace MBS {
	/// <summary>
	/// This specifies what is to happen on the server end, based on the numeric value of the enum. 
	/// Feel free to change the actual values if you want but do NOT change the ORDER of them.
	/// </summary>
	public enum eSASAction {Create, Award, Fetch}
	
	/// <summary>
	/// A very simple solution to creating online achievements systems.
	/// It consists of 3 main functions
	/// 
	/// Functions
	/// ---------
	/// 1. CreateNewAchievement() is used to store a new achievable item on the server
	/// 2. AwardAchievement() is used to indicate that a user has been awarded a specific achievement
	/// 3. FetchAchievements() is used to fetch all achievements and indicate which ones the user has
	/// 
	/// </summary>
	public class SASAchieveSDK : MonoBehaviour {
		
		public string 
			secret_salt_value	= "encryptionSalt",
			offline_url		 	= "localhost/Achievements/achievements.php",
			online_url 		 	= "htp://www.mysite.com/php/achievements.php";
		
		public bool
			use_online_url = false;
		
		public int GameID = 1;
		
		/// <summary>
		/// This function takes a normal string and returns it encoded into MD5.
		/// I made it static in case you want to use it in other scripts also
		/// </summary>
		/// <returns>The encoded string</returns>
		/// <param name="str">Any plain text</param>
		static public string MD5(string str)
		{
			UTF8Encoding encoding = new UTF8Encoding();
			byte[] bytes = encoding.GetBytes(str);
			
			// encrypt bytes
			System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
			byte[] hashBytes = md5.ComputeHash(bytes);
			
			// Convert the encrypted bytes back to a string (base 16)
			string hashString = string.Empty;
			
			for (int i = 0; i < hashBytes.Length; i++)
				hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, "0"[0]);
			
			return hashString.PadLeft(32, "0"[0]);
		}
		
		
		/// <summary>
		/// Creates a new achievement on the server. Everything is stored as strings so you can be a
		/// bit creative here if you like. If you store the icon's names with their extensions then
		/// you can use the icon name field to fetch graphics from the server and don't have to include
		/// any achievement graphics in your game. 
		/// 
		/// Alternatively you could store it without the extension and just load it via Resources.Load
		/// 
		/// Or if you already have the achievements in a list somewhere you could simply save the array
		/// index instead of the file name... it's entirely up to you how you choose to store it.
		/// 
		/// Once the data is fetched from the server I allow you to fetch the stored details both with
		/// and without file extensions, as numeric values(if possible) or as Sprites...
		/// 
		/// So it all depends on what your project needs...
		/// </summary>
		/// <param name="name">What do you want to call this achievement?</param>
		/// <param name="descr">Show this to players after they have won this award: "You are King of the Hill"</param>
		/// <param name="requirements">Explain to players hwo to get this award "Climb 500 feet up a hill"</param>
		/// <param name="icon_name">As explained in the description. This pertains to the graphic if they have the achievement</param>
		/// <param name="locked_icon">As explained in the description. This pertains to the graphic if they DONT have the achievement</param>
		/// <param name="onSuccess">A function to call if all works out well</param>
		/// <param name="onFail">A function to call in case somethig goes wrong</param>
		public void CreateNewAchievement(string name, string descr, string requirements, string icon_name, string locked_icon = "", Action<object> onSuccess = null, Action<string> onFail = null)
		{
			Dictionary<string, string>	fields = new Dictionary<string, string>();
			fields.Add("name"	, name);
			fields.Add("descr"	, descr);
			fields.Add("req"	, requirements);
			fields.Add("icongr"	, locked_icon);
			fields.Add("iconcol", icon_name);
			
			StartCoroutine( ContactServer(eSASAction.Create, fields, onSuccess, onFail) );
		}
		
		/// <summary>
		/// Award a player a specific achievement. Players are identified by their player_id which is a string
		/// field but obviously needs to be unique so simply letting them enter anyname they choose won't work
		/// out very well when two people with the same name starts playing your game. 
		/// 
		/// It is up to you to figure out a way to make each player's id value unique. For instance, if you use my
		/// Wordpress Login kit to log the player in with then you can use the username they used during login
		/// since all WP usernames have to be unique. That is one exmple...
		/// </summary>
		/// <param name="achievement">Achievement's unique id value as found on the server</param>
		/// <param name="onSuccess">A function to call if all went well</param>
		/// <param name="onFail">A function to call in case something went wrong</param>
		public void AwardAchievement(int achievement, string player_id, Action<object> onSuccess = null, Action<string> onFail = null)
		{
			Dictionary<string, string>	fields = new Dictionary<string, string>();
			fields.Add("aid"	, achievement.ToString());
			fields.Add("uid"	, player_id);
			
			StartCoroutine( ContactServer(eSASAction.Award, fields, onSuccess, onFail) );
		}
		
		/// <summary>
		/// This function will returns all achievements earned by a player named '0'. 
		/// Player '0' is awarded every achievement as soon as it's made with the CreateNewAchievement function
		/// so it basicaly serves to give you a list of all achievements that have been created for the system.
		/// 
		/// This was not the intende behavior but it works out nicely. You now have a choice:
		/// 1. You could fetch all achievements using this function, store all of them externally
		/// 	and then when you want to fetch a player's achievements you need only fetch the 'aid' field.
		/// 	All info is already stored so the 'aid' field can be used in a loop to determine
		/// 	"Have this, don't have this, have this" etc
		/// 2. Don't use this function at all and just fetch the player's achievements and ask for all details
		/// 	to be fetched at that time. 
		/// </summary>
		/// <param name="onSuccess">this will return a list of SASAchievement</param>
		/// <param name="onFail">Shows an error message if the search found no achievements</param>
		public void FetchAllAchievements(Action<object> onSuccess, Action<string> onFail = null)
		{
			Dictionary<string, string>	fields = new Dictionary<string, string>();
			fields.Add("uid"	, "0");
			fields.Add("subset"	, "*");
			
			StartCoroutine( ContactServer(eSASAction.Fetch, fields, onSuccess, onFail) );
		}
		
		/// <summary>
		/// Fetchs the achievements of a specific player. player_id is a case sensitive string
		/// </summary>
		/// <param name="player_id">Player_id.</param>
		/// <param name="onSuccess">On success.</param>
		/// <param name="onFail">On fail.</param>
		/// <param name="FetchName">If set to <c>true</c> fetch name.</param>
		/// <param name="FetchDescription">If set to <c>true</c> fetch description.</param>
		/// <param name="FetchRequirements">If set to <c>true</c> fetch requirements.</param>
		/// <param name="FetchGrayIcon">If set to <c>true</c> fetch gray icon name.</param>
		/// <param name="FetchColorIcon">If set to <c>true</c> fetch color icon name.</param>
		public void FetchAchievements(string player_id, Action<object> onSuccess, Action<string> onFail = null, bool FetchName = false, bool FetchDescription = false, bool FetchRequirements = false, bool FetchGrayIcon = false, bool FetchColorIcon = false)
		{
			string subset = string.Empty;
			
			if (FetchName) AddToList(ref subset, "name");
			if (FetchDescription) AddToList(ref subset, "descr");
			if (FetchRequirements) AddToList(ref subset, "req");
			if (FetchGrayIcon) AddToList(ref subset, "icongr");
			if (FetchColorIcon) AddToList(ref subset, "iconcol");
			if (subset == String.Empty) subset = "*";
			
			Dictionary<string, string>	fields = new Dictionary<string, string>();
			fields.Add("uid"	, player_id);
			fields.Add("subset"	, subset);
			
			StartCoroutine( ContactServer(eSASAction.Fetch, fields, onSuccess, onFail) );
		}
		
		#region private functions. Don't worry about this stuff
		IEnumerator ContactServer(eSASAction action, Dictionary<string,string> data, Action<object> onSuccess = null, Action<string> onFail = null)
		{
			var form = new WWWForm();
			string[] Tokens = GenerateToken();
			
			form.AddField( "token"	, Tokens[0]);
			form.AddField( "key"	, Tokens[1]);
			form.AddField( "online"	, use_online_url ? 1 : 0);
			form.AddField( "gid"	, GameID);
			form.AddField( "action"	, (int)action);
			
			foreach (KeyValuePair<string,string> entry in data)
			{
				form.AddField(entry.Key, entry.Value);
			}
			
			WWW w = new WWW(ScriptPath(), form);
			yield return w;
			
			if (null != w.error) {
				if (null != onFail)
					onFail("Error contacting server: " + w.error);
			}
			else
			{
				
				string formText = w.text; 
				w.Dispose();
				
				string[] fields = formText.Split(':');
				int errorCode = 0;
				if(!int.TryParse(fields[0], out errorCode))
				{
					errorCode = 8;
				}
				
				switch(errorCode)
				{
				case 0:
					if (null != onSuccess)
					{
						if (action == eSASAction.Fetch)
						{
							List<SASAchievement> achievements = new List<SASAchievement>();
							for( int i = 1; i < fields.Length; i++)
								achievements.Add(ParseAchievements( fields[i]) );
							onSuccess(achievements);
						} else
							onSuccess(fields[1]);
					}
					break;
					
				case 8:
					if (null != onFail)
						onFail("Invalid server response: " + formText);
					break;
					
				case 9:
					if (null != onFail)
						onFail("Unable to connect to server. Check username and password");
					break;
					
				default:
					if (null != onFail)
						onFail(fields[1]);
					break;
				}
			}
		}
		
		string ScriptPath() {
			return use_online_url ? online_url : offline_url;
		}
		
		string[] GenerateToken()
		{
			//the order of everything here is important so don't change anything
			//unless you are 100% sure you know what you are doing.
			string[] Tokens = new string[2];
			Tokens[0] = MD5(secret_salt_value + Time.time + Time.deltaTime);
			Tokens[1] = MD5(Tokens[0] + secret_salt_value);
			
			return Tokens;
		}
		
		SASAchievement ParseAchievements(string raw)
		{
			string[] elements = raw.Split(',');
			string 
				n = string.Empty,
				d = string.Empty,
				r = string.Empty,
				gray = string.Empty,
				color = string.Empty;
			
			foreach(string s in elements) {
				string[] keyValue = s.Split('=');
				switch (keyValue[0]) {
				case "name":	n		= keyValue[1];	break;
				case "descr":	d		= keyValue[1];	break;
				case "req":		r		= keyValue[1];	break;
				case "icongr":	gray	= keyValue[1];	break;
				case "iconcol":	color	= keyValue[1];	break;
				}
			}
			SASAchievement data = new SASAchievement(n,d,r,gray,color);
			return data;
		}
		
		void AddToList(ref string list, string item)
		{
			if (list == string.Empty) list = item; else list += ","+item;
		}
		#endregion
	}
	
	/// <summary>
	/// This data structure stores the achievement data returned from the server
	/// 
	/// This system really is made to very simple but also made to be very flexible at the same time
	/// For instance, if you store your achievement's icons in an array in your game somewhere,
	/// instead of saving the icon's name, you could save the array index instead. This class
	/// makes provision for you to fetch the name and icons as either text or numeric values.
	/// 
	/// Additionally, it has the option of loading your icons as sprites directly and returning that also
	/// so really, depending your needs you can save your achievements via a variety of methods...
	/// </summary>
	public class SASAchievement {
		public string
			Name,			
			Description,		
			Requirements,	
			GrayIcon,		
			ColorIcon;		
		
		public Sprite
			LockedImg,
			UnlockedImg;
		
		/// <summary>
		/// If you saved the texture name with file extention, this returnes it without it
		/// </summary>
		/// <value>The name of the locked graphic.</value>
		public string LockedGraphicName { get { return GrayIcon == string.Empty ? GrayIcon : ResourceFilename(GrayIcon); } }
		/// <summary>
		/// If you saved the texture name with file extention, this returnes it without it
		/// </summary>
		/// <value>The name of the locked graphic.</value>
		public string UnlockedGraphicName { get { return ColorIcon == string.Empty ? ColorIcon : ResourceFilename(ColorIcon); } }
		
		public SASAchievement(string name, string descr, string req, string locked_graphic, string unlocked_graphic) {
			LockedImg = UnlockedImg = null;
			Name = name;
			Description = descr;
			Requirements = req;
			SetLockedGraphic(locked_graphic);
			SetUnlockedGraphic(unlocked_graphic);
		}
		
		/// <summary>
		/// If you saved the name as a numeric value, this returns it as a number or as -1 if not possible
		/// </summary>
		public int Namei 
		{ get {
				int result = 0;
				if (!int.TryParse(Name, out result))
					result =-1;
				return result;
			}
		}
		
		/// <summary>
		/// If you saved the graphic name for the locked achievement as a numeric value, this returns it as a number or as -1 if not possible
		/// </summary>
		public int InactiveIconi
		{ get {
				int result = 0;
				if (!int.TryParse(GrayIcon, out result))
					result =-1;
				return result;
			}
		}
		
		/// <summary>
		/// If you saved the graphic name for the unlocked achievement as a numeric value, this returns it as a number or as -1 if not possible
		/// </summary>
		public int ActiveIconi
		{ get {
				int result = 0;
				if (!int.TryParse(ColorIcon, out result))
					result =-1;
				return result;
			}
		}
		
		string ResourceFilename(string s)
		{
			if (s.LastIndexOf(".") == s.Length - 4)
				return s.Substring(0, s.Length - 4);
			
			return s;
		}
		
		void SetLockedGraphic(string name)
		{
			GrayIcon = name;
			if (GrayIcon != string.Empty)
				LockedImg = Resources.Load<Sprite>(ResourceFilename(name));
		}
		
		void SetUnlockedGraphic(string name)
		{
			ColorIcon = name.Trim();
			if (ColorIcon != string.Empty)
				UnlockedImg = Resources.Load<Sprite>(ResourceFilename(name));
		}
		
	}
	
}