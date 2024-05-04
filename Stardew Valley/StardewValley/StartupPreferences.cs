using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using StardewValley.GameData;

namespace StardewValley;

public class StartupPreferences
{
	public const int windowed_borderless = 0;

	public const int windowed = 1;

	public const int fullscreen = 2;

	private static readonly string _filename = "startup_preferences";

	public static XmlSerializer serializer = new XmlSerializer(typeof(StartupPreferences));

	public bool startMuted;

	public bool levelTenFishing;

	public bool levelTenMining;

	public bool levelTenForaging;

	public bool levelTenCombat;

	public bool skipWindowPreparation;

	public bool sawAdvancedCharacterCreationIndicator;

	public int timesPlayed;

	public int windowMode;

	public int displayIndex = -1;

	public Options.GamepadModes gamepadMode;

	public int playerLimit = -1;

	public int fullscreenResolutionX;

	public int fullscreenResolutionY;

	public string lastEnteredIP = "";

	public string languageCode;

	public Options clientOptions = new Options();

	[XmlIgnore]
	public bool isLoaded;

	private bool _isBusy;

	private bool _pendingApplyLanguage;

	private Task _task;

	[XmlIgnore]
	public bool IsBusy
	{
		get
		{
			lock (this)
			{
				if (!_isBusy)
				{
					return false;
				}
				if (_task == null)
				{
					throw new Exception("StartupPreferences.IsBusy; was busy but task is null?");
				}
				if (_task.IsFaulted)
				{
					Exception e = _task.Exception.GetBaseException();
					Game1.log.Error("StartupPreferences._task failed with an exception.", e);
					throw e;
				}
				if (_task.IsCompleted)
				{
					_task = null;
					_isBusy = false;
					if (_pendingApplyLanguage)
					{
						_SetLanguageFromCode(languageCode);
					}
				}
				return _isBusy;
			}
		}
	}

	private void Init()
	{
		isLoaded = false;
		ensureFolderStructureExists();
	}

	public void OnLanguageChange(LocalizedContentManager.LanguageCode code)
	{
		string language_id = code.ToString();
		if (code == LocalizedContentManager.LanguageCode.mod && LocalizedContentManager.CurrentModLanguage != null)
		{
			language_id = LocalizedContentManager.CurrentModLanguage.Id;
		}
		if (isLoaded && languageCode != language_id)
		{
			savePreferences(async: false, update_language_from_ingame_language: true);
		}
	}

	private void ensureFolderStructureExists()
	{
		Program.GetAppDataFolder();
	}

	public void savePreferences(bool async, bool update_language_from_ingame_language = false)
	{
		lock (this)
		{
			if (update_language_from_ingame_language)
			{
				if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod)
				{
					languageCode = LocalizedContentManager.CurrentModLanguage.Id;
				}
				else
				{
					languageCode = LocalizedContentManager.CurrentLanguageCode.ToString();
				}
			}
			try
			{
				_savePreferences();
			}
			catch (Exception ex)
			{
				Game1.log.Error("StartupPreferences._task failed with an exception.", ex);
				throw ex;
			}
		}
	}

	private void _savePreferences()
	{
		string fullFilePath = Path.Combine(Program.GetAppDataFolder(), _filename);
		try
		{
			ensureFolderStructureExists();
			if (File.Exists(fullFilePath))
			{
				File.Delete(fullFilePath);
			}
			using FileStream stream = File.Create(fullFilePath);
			writeSettings(stream);
		}
		catch (Exception ex)
		{
			Game1.debugOutput = Game1.parseText(ex.Message);
		}
	}

	private long writeSettings(Stream stream)
	{
		XmlWriterSettings settings = new XmlWriterSettings
		{
			CloseOutput = true,
			Indent = true
		};
		using XmlWriter writer = XmlWriter.Create(stream, settings);
		writer.WriteStartDocument();
		serializer.Serialize(writer, this);
		writer.WriteEndDocument();
		writer.Flush();
		return stream.Length;
	}

	public void loadPreferences(bool async, bool applyLanguage)
	{
		lock (this)
		{
			_pendingApplyLanguage = applyLanguage;
			Init();
			try
			{
				_loadPreferences();
			}
			catch (Exception ex)
			{
				Exception e = _task.Exception?.GetBaseException() ?? ex;
				Game1.log.Error("StartupPreferences._task failed with an exception.", e);
				throw e;
			}
			if (applyLanguage)
			{
				_SetLanguageFromCode(languageCode);
			}
		}
	}

	protected virtual void _SetLanguageFromCode(string language_code_string)
	{
		List<ModLanguage> mod_languages = DataLoader.AdditionalLanguages(Game1.content);
		bool found_language = false;
		if (mod_languages != null)
		{
			foreach (ModLanguage mod_language in mod_languages)
			{
				if (mod_language.Id == language_code_string)
				{
					LocalizedContentManager.SetModLanguage(mod_language);
					found_language = true;
					break;
				}
			}
		}
		if (!found_language)
		{
			if (Utility.TryParseEnum<LocalizedContentManager.LanguageCode>(language_code_string, out var language_code) && language_code != LocalizedContentManager.LanguageCode.mod)
			{
				LocalizedContentManager.CurrentLanguageCode = language_code;
			}
			else
			{
				LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.GetDefaultLanguageCode();
			}
		}
	}

	private void _loadPreferences()
	{
		string fullFilePath = Path.Combine(Program.GetAppDataFolder(), _filename);
		if (!File.Exists(fullFilePath))
		{
			Game1.log.Verbose("path '" + fullFilePath + "' did not exist and will be created");
			try
			{
				if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod)
				{
					languageCode = LocalizedContentManager.CurrentModLanguage.Id;
				}
				else
				{
					languageCode = LocalizedContentManager.CurrentLanguageCode.ToString();
				}
				using FileStream stream2 = File.Create(fullFilePath);
				writeSettings(stream2);
			}
			catch (Exception e)
			{
				Game1.log.Error("_loadPreferences; exception occurred trying to create/write.", e);
				Game1.debugOutput = Game1.parseText(e.Message);
				return;
			}
		}
		try
		{
			using (FileStream stream = File.Open(fullFilePath, FileMode.Open, FileAccess.Read))
			{
				readSettings(stream);
			}
			isLoaded = true;
		}
		catch (Exception e)
		{
			Game1.log.Error("_loadPreferences; exception occurred trying open/read.", e);
			Game1.debugOutput = Game1.parseText(e.Message);
		}
	}

	private void readSettings(Stream stream)
	{
		StartupPreferences p = (StartupPreferences)serializer.Deserialize(stream);
		startMuted = p.startMuted;
		timesPlayed = p.timesPlayed + 1;
		levelTenCombat = p.levelTenCombat;
		levelTenFishing = p.levelTenFishing;
		levelTenForaging = p.levelTenForaging;
		levelTenMining = p.levelTenMining;
		skipWindowPreparation = p.skipWindowPreparation;
		windowMode = p.windowMode;
		displayIndex = p.displayIndex;
		playerLimit = p.playerLimit;
		gamepadMode = p.gamepadMode;
		fullscreenResolutionX = p.fullscreenResolutionX;
		fullscreenResolutionY = p.fullscreenResolutionY;
		lastEnteredIP = p.lastEnteredIP;
		languageCode = p.languageCode;
		clientOptions = p.clientOptions;
	}
}
