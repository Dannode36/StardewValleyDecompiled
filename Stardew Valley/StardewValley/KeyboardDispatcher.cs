using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley.NativeClipboard;

namespace StardewValley;

public class KeyboardDispatcher
{
	protected string _enteredText;

	protected List<char> _commandInputs = new List<char>();

	protected List<Keys> _keysDown = new List<Keys>();

	protected List<char> _charsEntered = new List<char>();

	protected GameWindow _window;

	protected KeyboardState _oldKeyboardState;

	private IKeyboardSubscriber _subscriber;

	private string _pasteResult = "";

	public IKeyboardSubscriber Subscriber
	{
		get
		{
			return _subscriber;
		}
		set
		{
			if (_subscriber != value)
			{
				if (_subscriber != null)
				{
					_subscriber.Selected = false;
				}
				_subscriber = value;
				if (_subscriber != null)
				{
					_subscriber.Selected = true;
				}
			}
		}
	}

	public void Cleanup()
	{
		if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.Win32NT)
		{
			_window.TextInput -= Event_TextInput;
		}
		else
		{
			KeyboardInput.CharEntered -= EventInput_CharEntered;
			KeyboardInput.KeyDown -= EventInput_KeyDown;
		}
		_window = null;
	}

	public KeyboardDispatcher(GameWindow window)
	{
		_commandInputs = new List<char>();
		_keysDown = new List<Keys>();
		_charsEntered = new List<char>();
		_window = window;
		if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.Win32NT)
		{
			window.TextInput += Event_TextInput;
			return;
		}
		if (Game1.game1.IsMainInstance)
		{
			KeyboardInput.Initialize(window);
		}
		KeyboardInput.CharEntered += EventInput_CharEntered;
		KeyboardInput.KeyDown += EventInput_KeyDown;
	}

	private void Event_KeyDown(object sender, Keys key)
	{
		if (_subscriber != null)
		{
			switch (key)
			{
			case Keys.Back:
				_commandInputs.Add('\b');
				break;
			case Keys.Enter:
				_commandInputs.Add('\r');
				break;
			case Keys.Tab:
				_commandInputs.Add('\t');
				break;
			}
			_keysDown.Add(key);
		}
	}

	private void Event_TextInput(object sender, TextInputEventArgs e)
	{
		if (_subscriber == null)
		{
			return;
		}
		switch (e.Key)
		{
		case Keys.Back:
			_commandInputs.Add('\b');
			return;
		case Keys.Enter:
			_commandInputs.Add('\r');
			return;
		case Keys.Tab:
			_commandInputs.Add('\t');
			return;
		}
		if (!char.IsControl(e.Character))
		{
			_charsEntered.Add(e.Character);
		}
	}

	private void EventInput_KeyDown(object sender, KeyEventArgs e)
	{
		_keysDown.Add(e.KeyCode);
	}

	private void EventInput_CharEntered(object sender, CharacterEventArgs e)
	{
		if (_subscriber == null)
		{
			return;
		}
		if (char.IsControl(e.Character))
		{
			if (e.Character == '\u0016')
			{
				Thread thread = new Thread(PasteThread);
				thread.SetApartmentState(ApartmentState.STA);
				thread.Start();
				thread.Join();
				_enteredText = _pasteResult;
			}
			else
			{
				_commandInputs.Add(e.Character);
			}
		}
		else
		{
			_charsEntered.Add(e.Character);
		}
	}

	public bool ShouldSuppress()
	{
		return false;
	}

	public void Discard()
	{
		_enteredText = null;
		_charsEntered.Clear();
		_commandInputs.Clear();
		_keysDown.Clear();
	}

	public void Poll()
	{
		KeyboardState keyboard_state = Game1.input.GetKeyboardState();
		bool modifier_held = ((SdlClipboard.Platform != ClipboardPlatformType.OSX) ? (keyboard_state.IsKeyDown(Keys.LeftControl) || keyboard_state.IsKeyDown(Keys.RightControl)) : (keyboard_state.IsKeyDown(Keys.LeftWindows) || keyboard_state.IsKeyDown(Keys.RightWindows)));
		if (keyboard_state.IsKeyDown(Keys.V) && !_oldKeyboardState.IsKeyDown(Keys.V) && modifier_held)
		{
			string pasted_text = null;
			DesktopClipboard.GetText(ref pasted_text);
			if (pasted_text != null)
			{
				_enteredText = pasted_text;
			}
		}
		_oldKeyboardState = keyboard_state;
		if (_enteredText != null)
		{
			if (_subscriber != null && !ShouldSuppress())
			{
				_subscriber.RecieveTextInput(_enteredText);
			}
			_enteredText = null;
		}
		if (_charsEntered.Count > 0)
		{
			if (_subscriber != null && !ShouldSuppress())
			{
				foreach (char key in _charsEntered)
				{
					_subscriber.RecieveTextInput(key);
					if (_subscriber == null)
					{
						break;
					}
				}
			}
			_charsEntered.Clear();
		}
		if (_commandInputs.Count > 0)
		{
			if (_subscriber != null && !ShouldSuppress())
			{
				foreach (char key in _commandInputs)
				{
					_subscriber.RecieveCommandInput(key);
					if (_subscriber == null)
					{
						break;
					}
				}
			}
			_commandInputs.Clear();
		}
		if (_keysDown.Count <= 0)
		{
			return;
		}
		if (_subscriber != null && !ShouldSuppress())
		{
			foreach (Keys key in _keysDown)
			{
				_subscriber.RecieveSpecialInput(key);
				if (_subscriber == null)
				{
					break;
				}
			}
		}
		_keysDown.Clear();
	}

	[STAThread]
	private void PasteThread()
	{
		_pasteResult = "";
	}
}
