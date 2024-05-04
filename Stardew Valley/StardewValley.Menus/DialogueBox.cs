using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus;

public class DialogueBox : IClickableMenu
{
	public List<string> dialogues = new List<string>();

	public Dialogue characterDialogue;

	public Stack<string> characterDialoguesBrokenUp = new Stack<string>();

	public Response[] responses = LegacyShims.EmptyArray<Response>();

	public const int portraitBoxSize = 74;

	public const int nameTagWidth = 102;

	public const int nameTagHeight = 18;

	public const int portraitPlateWidth = 115;

	public const int nameTagSideMargin = 5;

	public const float transitionRate = 3f;

	public const int characterAdvanceDelay = 30;

	public const int safetyDelay = 750;

	public int questionFinishPauseTimer;

	protected bool _showedOptions;

	public Rectangle friendshipJewel = Rectangle.Empty;

	public List<ClickableComponent> responseCC;

	public int x;

	public int y;

	public int transitionX = -1;

	public int transitionY;

	public int transitionWidth;

	public int transitionHeight;

	public int characterAdvanceTimer;

	public int characterIndexInDialogue;

	public int safetyTimer = 750;

	public int heightForQuestions;

	public int selectedResponse = -1;

	public int newPortaitShakeTimer;

	public bool transitionInitialized;

	/// <summary>Whether to progressively type the dialogue text into the box. If false, the dialogue appears instantly instead.</summary>
	public bool showTyping = true;

	public bool transitioning = true;

	public bool transitioningBigger = true;

	public bool dialogueContinuedOnNextPage;

	public bool dialogueFinished;

	public bool isQuestion;

	public TemporaryAnimatedSprite dialogueIcon;

	public TemporaryAnimatedSprite aboveDialogueImage;

	private string hoverText = "";

	public DialogueBox(int x, int y, int width, int height)
	{
		if (Game1.options.SnappyMenus)
		{
			Game1.mouseCursorTransparency = 0f;
		}
		this.x = x;
		this.y = y;
		base.width = width;
		base.height = height;
	}

	public DialogueBox(string dialogue)
	{
		if (Game1.options.SnappyMenus)
		{
			Game1.mouseCursorTransparency = 0f;
		}
		dialogues.AddRange(dialogue.Split('#'));
		width = Math.Min(1240, SpriteText.getWidthOfString(dialogues[0]) + 64);
		height = SpriteText.getHeightOfString(dialogues[0], width - 20) + 4;
		x = (int)Utility.getTopLeftPositionForCenteringOnScreen(width, height).X;
		y = Game1.uiViewport.Height - height - 64;
		setUpIcons();
	}

	public DialogueBox(string dialogue, Response[] responses, int width = 1200)
	{
		if (Game1.options.SnappyMenus)
		{
			Game1.mouseCursorTransparency = 0f;
		}
		dialogues.Add(dialogue);
		this.responses = responses;
		isQuestion = true;
		base.width = width;
		setUpQuestions();
		height = heightForQuestions;
		x = (int)Utility.getTopLeftPositionForCenteringOnScreen(width, height).X;
		y = Game1.uiViewport.Height - height - 64;
		setUpIcons();
		characterIndexInDialogue = dialogue.Length;
		if (responses != null)
		{
			foreach (Response response in responses)
			{
				response.responseText = Dialogue.applyGenderSwitch(Game1.player.Gender, response.responseText, altTokenOnly: true);
			}
		}
	}

	public DialogueBox(Dialogue dialogue)
	{
		if (Game1.options.SnappyMenus)
		{
			Game1.mouseCursorTransparency = 0f;
		}
		characterDialogue = dialogue;
		width = 1200;
		height = 384;
		x = (int)Utility.getTopLeftPositionForCenteringOnScreen(width, height).X;
		y = Game1.uiViewport.Height - height - 64;
		friendshipJewel = new Rectangle(x + width - 64, y + 256, 44, 44);
		dialogue.prepareDialogueForDisplay();
		characterDialogue.prepareCurrentDialogueForDisplay();
		if (!characterDialogue.isDialogueFinished())
		{
			characterDialoguesBrokenUp.Push(dialogue.getCurrentDialogue());
			checkDialogue(dialogue);
		}
		else
		{
			dialogueFinished = true;
		}
		newPortaitShakeTimer = ((characterDialogue.getPortraitIndex() == 1) ? 250 : 0);
		setUpForGamePadMode();
	}

	public DialogueBox(List<string> dialogues)
	{
		if (Game1.options.SnappyMenus)
		{
			Game1.mouseCursorTransparency = 0f;
		}
		this.dialogues = dialogues;
		width = Math.Min(1200, SpriteText.getWidthOfString(dialogues[0]) + 64);
		height = SpriteText.getHeightOfString(dialogues[0], width - 16);
		x = (int)Utility.getTopLeftPositionForCenteringOnScreen(width, height).X;
		y = Game1.uiViewport.Height - height - 64;
		setUpIcons();
	}

	public override void snapToDefaultClickableComponent()
	{
		currentlySnappedComponent = getComponentWithID(0);
		snapCursorToCurrentSnappedComponent();
	}

	private void playOpeningSound()
	{
		Game1.playSound("breathin");
	}

	public override void setUpForGamePadMode()
	{
	}

	public void closeDialogue()
	{
		if (Game1.activeClickableMenu.Equals(this))
		{
			Game1.exitActiveMenu();
			Game1.dialogueUp = false;
			if (characterDialogue?.speaker != null && characterDialogue.speaker.CurrentDialogue.Count > 0 && dialogueFinished && characterDialogue.speaker.CurrentDialogue.Count > 0)
			{
				characterDialogue.speaker.CurrentDialogue.Pop();
			}
			if (Game1.messagePause)
			{
				Game1.pauseTime = 500f;
			}
			if (Game1.currentObjectDialogue.Count > 0)
			{
				Game1.currentObjectDialogue.Dequeue();
			}
			Game1.currentDialogueCharacterIndex = 0;
			if (Game1.currentObjectDialogue.Count > 0)
			{
				Game1.dialogueUp = true;
				Game1.questionChoices.Clear();
				Game1.dialogueTyping = true;
			}
			if (characterDialogue?.speaker != null && !characterDialogue.speaker.Name.Equals("Gunther") && !Game1.eventUp && !characterDialogue.speaker.doingEndOfRouteAnimation)
			{
				characterDialogue.speaker.doneFacingPlayer(Game1.player);
			}
			Game1.currentSpeaker = null;
			if (!Game1.eventUp)
			{
				if (!Game1.isWarping)
				{
					Game1.player.CanMove = true;
				}
				Game1.player.movementDirections.Clear();
			}
			else if (Game1.currentLocation.currentEvent.CurrentCommand > 0 || Game1.currentLocation.currentEvent.specialEventVariable1)
			{
				if (!Game1.isFestival() || !Game1.currentLocation.currentEvent.canMoveAfterDialogue())
				{
					Game1.currentLocation.currentEvent.CurrentCommand++;
				}
				else
				{
					Game1.player.CanMove = true;
				}
			}
			Game1.questionChoices.Clear();
		}
		if (Game1.afterDialogues != null)
		{
			Game1.afterFadeFunction afterDialogues = Game1.afterDialogues;
			Game1.afterDialogues = null;
			afterDialogues();
		}
	}

	public void finishTyping()
	{
		characterIndexInDialogue = getCurrentString().Length;
	}

	public void beginOutro()
	{
		transitioning = true;
		transitioningBigger = false;
		Game1.playSound("breathout");
	}

	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		receiveLeftClick(x, y, playSound);
	}

	private void tryOutro()
	{
		if (Game1.activeClickableMenu != null && Game1.activeClickableMenu.Equals(this))
		{
			beginOutro();
		}
	}

	public override void receiveKeyPress(Keys key)
	{
		if (transitioning)
		{
			return;
		}
		if (Game1.options.SnappyMenus && !isQuestion && Game1.options.doesInputListContain(Game1.options.menuButton, key))
		{
			receiveLeftClick(0, 0);
		}
		else if (!Game1.options.gamepadControls && Game1.options.doesInputListContain(Game1.options.actionButton, key))
		{
			receiveLeftClick(0, 0);
		}
		else if (isQuestion && !Game1.eventUp && characterDialogue == null)
		{
			if (responses != null)
			{
				Response[] array = responses;
				foreach (Response response in array)
				{
					if (response.hotkey == key && Game1.currentLocation.answerDialogue(response))
					{
						Game1.playSound("smallSelect");
						selectedResponse = -1;
						tryOutro();
						return;
					}
				}
				if (key == Keys.N)
				{
					array = responses;
					foreach (Response response in array)
					{
						if (response.hotkey == Keys.Escape && Game1.currentLocation.answerDialogue(response))
						{
							Game1.playSound("smallSelect");
							selectedResponse = -1;
							tryOutro();
							return;
						}
					}
				}
			}
			if (Game1.options.doesInputListContain(Game1.options.menuButton, key) || key == Keys.N)
			{
				Response[] array2 = responses;
				if (array2 != null && array2.Length != 0 && Game1.currentLocation.answerDialogue(responses[responses.Length - 1]))
				{
					Game1.playSound("smallSelect");
				}
				selectedResponse = -1;
				tryOutro();
			}
			else if (Game1.options.SnappyMenus)
			{
				safetyTimer = 0;
				base.receiveKeyPress(key);
			}
			else if (key == Keys.Y)
			{
				Response[] array3 = responses;
				if (array3 != null && array3.Length != 0 && responses[0].responseKey.Equals("Yes") && Game1.currentLocation.answerDialogue(responses[0]))
				{
					Game1.playSound("smallSelect");
					selectedResponse = -1;
					tryOutro();
				}
			}
		}
		else if (Game1.options.SnappyMenus && isQuestion && !Game1.options.doesInputListContain(Game1.options.menuButton, key))
		{
			base.receiveKeyPress(key);
		}
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (transitioning)
		{
			return;
		}
		if (characterIndexInDialogue < getCurrentString().Length - 1)
		{
			characterIndexInDialogue = getCurrentString().Length - 1;
		}
		else
		{
			if (safetyTimer > 0)
			{
				return;
			}
			if (isQuestion)
			{
				if (selectedResponse == -1)
				{
					return;
				}
				questionFinishPauseTimer = (Game1.eventUp ? 600 : 200);
				transitioning = true;
				transitionInitialized = false;
				transitioningBigger = true;
				if (characterDialogue == null)
				{
					Game1.dialogueUp = false;
					if (Game1.eventUp && Game1.currentLocation.afterQuestion == null)
					{
						Game1.playSound("smallSelect");
						Game1.currentLocation.currentEvent.answerDialogue(Game1.currentLocation.lastQuestionKey, selectedResponse);
						selectedResponse = -1;
						tryOutro();
						return;
					}
					if (Game1.currentLocation.answerDialogue(responses[selectedResponse]))
					{
						Game1.playSound("smallSelect");
					}
					selectedResponse = -1;
					tryOutro();
					return;
				}
				characterDialoguesBrokenUp.Pop();
				characterDialogue.chooseResponse(responses[selectedResponse]);
				characterDialoguesBrokenUp.Push("");
				Game1.playSound("smallSelect");
			}
			else if (characterDialogue == null)
			{
				dialogues.RemoveAt(0);
				if (dialogues.Count == 0)
				{
					closeDialogue();
				}
				else
				{
					width = Math.Min(1200, SpriteText.getWidthOfString(dialogues[0]) + 64);
					height = SpriteText.getHeightOfString(dialogues[0], width - 16);
					this.x = (int)Utility.getTopLeftPositionForCenteringOnScreen(width, height).X;
					this.y = Game1.uiViewport.Height - height - 64;
					xPositionOnScreen = x;
					yPositionOnScreen = y;
					setUpIcons();
				}
			}
			characterIndexInDialogue = 0;
			if (characterDialogue != null)
			{
				int oldPortrait = characterDialogue.getPortraitIndex();
				if (characterDialoguesBrokenUp.Count == 0)
				{
					beginOutro();
					return;
				}
				characterDialoguesBrokenUp.Pop();
				if (characterDialoguesBrokenUp.Count == 0)
				{
					if (!characterDialogue.isCurrentStringContinuedOnNextScreen)
					{
						beginOutro();
					}
					characterDialogue.exitCurrentDialogue();
				}
				if (!characterDialogue.isDialogueFinished() && characterDialogue.getCurrentDialogue().Length > 0 && characterDialoguesBrokenUp.Count == 0)
				{
					characterDialogue.prepareCurrentDialogueForDisplay();
					if (characterDialogue.isDialogueFinished())
					{
						beginOutro();
						return;
					}
					characterDialoguesBrokenUp.Push(characterDialogue.getCurrentDialogue());
				}
				checkDialogue(characterDialogue);
				if (characterDialogue.getPortraitIndex() != oldPortrait)
				{
					newPortaitShakeTimer = ((characterDialogue.getPortraitIndex() == 1) ? 250 : 50);
				}
			}
			if (!transitioning)
			{
				Game1.playSound("smallSelect");
			}
			setUpIcons();
			safetyTimer = 750;
			if (getCurrentString() != null && getCurrentString().Length <= 20)
			{
				safetyTimer -= 200;
			}
		}
	}

	private void setUpIcons()
	{
		dialogueIcon = null;
		if (isQuestion)
		{
			setUpQuestionIcon();
		}
		else if (characterDialogue != null && (characterDialogue.isCurrentStringContinuedOnNextScreen || characterDialoguesBrokenUp.Count > 1))
		{
			setUpNextPageIcon();
		}
		else
		{
			List<string> list = dialogues;
			if (list != null && list.Count > 1)
			{
				setUpNextPageIcon();
			}
			else
			{
				setUpCloseDialogueIcon();
			}
		}
		setUpForGamePadMode();
		if (getCurrentString() != null && getCurrentString().Length <= 20)
		{
			safetyTimer -= 200;
		}
	}

	public override void performHoverAction(int mouseX, int mouseY)
	{
		hoverText = "";
		if (!transitioning && characterIndexInDialogue >= getCurrentString().Length - 1)
		{
			base.performHoverAction(mouseX, mouseY);
			if (isQuestion)
			{
				int oldResponse = selectedResponse;
				selectedResponse = -1;
				if (Game1.options.gamepadControls && currentlySnappedComponent != null)
				{
					selectedResponse = currentlySnappedComponent.myID;
				}
				int responseY = y - (heightForQuestions - height) + SpriteText.getHeightOfString(getCurrentString(), width - 16) + 48;
				int margin = 8;
				for (int i = 0; i < responses.Length; i++)
				{
					if (mouseY >= responseY - margin && mouseY < responseY + SpriteText.getHeightOfString(responses[i].responseText, width - 16) + margin)
					{
						selectedResponse = i;
						if (i < responseCC?.Count)
						{
							currentlySnappedComponent = responseCC[i];
						}
						break;
					}
					responseY += SpriteText.getHeightOfString(responses[i].responseText, width - 16) + 16;
				}
				if (selectedResponse != oldResponse)
				{
					Game1.playSound("Cowboy_gunshot");
				}
			}
		}
		if (shouldDrawFriendshipJewel() && friendshipJewel.Contains(mouseX, mouseY))
		{
			hoverText = Game1.player.getFriendshipHeartLevelForNPC(characterDialogue.speaker.Name) + "/" + Utility.GetMaximumHeartsForCharacter(characterDialogue.speaker) + "<";
		}
		if (Game1.options.SnappyMenus && currentlySnappedComponent != null)
		{
			selectedResponse = currentlySnappedComponent.myID;
		}
	}

	public bool shouldDrawFriendshipJewel()
	{
		if (width >= 642 && !Game1.eventUp && !isQuestion && isPortraitBox() && !friendshipJewel.Equals(Rectangle.Empty) && characterDialogue?.speaker != null && Game1.player.friendshipData.ContainsKey(characterDialogue.speaker.Name) && characterDialogue.speaker.Name != "Henchman")
		{
			return true;
		}
		return false;
	}

	private void setUpQuestionIcon()
	{
	}

	private void setUpCloseDialogueIcon()
	{
		Vector2 iconPosition = new Vector2(x + width - 40, y + height - 44);
		if (isPortraitBox())
		{
			iconPosition.X -= 492f;
		}
		dialogueIcon = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(289, 342, 11, 12), 80f, 11, 999999, iconPosition, flicker: false, flipped: false, 0.89f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true);
	}

	private void setUpNextPageIcon()
	{
		Vector2 iconPosition = new Vector2(x + width - 40, y + height - 40);
		if (isPortraitBox())
		{
			iconPosition.X -= 492f;
		}
		dialogueIcon = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(232, 346, 9, 9), 90f, 6, 999999, iconPosition, flicker: false, flipped: false, 0.89f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			yPeriodic = true,
			yPeriodicLoopTime = 1500f,
			yPeriodicRange = 8f
		};
	}

	private void checkDialogue(Dialogue d)
	{
		isQuestion = false;
		string sub = "";
		if (characterDialoguesBrokenUp.Count == 1)
		{
			sub = SpriteText.getSubstringBeyondHeight(characterDialoguesBrokenUp.Peek(), width - 460 - 20, height - 16);
		}
		if (sub.Length > 0)
		{
			string full = characterDialoguesBrokenUp.Pop().Replace(Environment.NewLine, "");
			characterDialoguesBrokenUp.Push(sub.Trim());
			characterDialoguesBrokenUp.Push(full.Substring(0, full.Length - sub.Length + 1).Trim());
		}
		if (d.getCurrentDialogue().Length == 0)
		{
			dialogueFinished = true;
		}
		if (d.isCurrentStringContinuedOnNextScreen || characterDialoguesBrokenUp.Count > 1)
		{
			dialogueContinuedOnNextPage = true;
		}
		else if (d.getCurrentDialogue().Length == 0)
		{
			beginOutro();
		}
		if (d.isCurrentDialogueAQuestion())
		{
			responses = d.getResponseOptions();
			isQuestion = true;
			setUpQuestions();
		}
	}

	private void setUpQuestions()
	{
		int tmpwidth = width - 16;
		heightForQuestions = SpriteText.getHeightOfString(getCurrentString(), tmpwidth);
		Response[] array = responses;
		foreach (Response r in array)
		{
			heightForQuestions += SpriteText.getHeightOfString(r.responseText, tmpwidth) + 16;
		}
		heightForQuestions += 40;
	}

	public bool isPortraitBox()
	{
		if (characterDialogue?.speaker?.Portrait != null && characterDialogue.showPortrait)
		{
			return Game1.options.showPortraits;
		}
		return false;
	}

	public void drawBox(SpriteBatch b, int xPos, int yPos, int boxWidth, int boxHeight)
	{
		if (transitionInitialized)
		{
			b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos, boxWidth, boxHeight), new Rectangle(306, 320, 16, 16), Color.White);
			b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos - 20, boxWidth, 24), new Rectangle(275, 313, 1, 6), Color.White);
			b.Draw(Game1.mouseCursors, new Rectangle(xPos + 12, yPos + boxHeight, boxWidth - 20, 32), new Rectangle(275, 328, 1, 8), Color.White);
			b.Draw(Game1.mouseCursors, new Rectangle(xPos - 32, yPos + 24, 32, boxHeight - 28), new Rectangle(264, 325, 8, 1), Color.White);
			b.Draw(Game1.mouseCursors, new Rectangle(xPos + boxWidth, yPos, 28, boxHeight), new Rectangle(293, 324, 7, 1), Color.White);
			b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos - 28), new Rectangle(261, 311, 14, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos - 28), new Rectangle(291, 311, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos + boxHeight - 8), new Rectangle(291, 326, 12, 12), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos + boxHeight - 4), new Rectangle(261, 327, 14, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
		}
	}

	/// <summary>Get whether the current portrait should shake.</summary>
	/// <param name="d">The dialogue being displayed.</param>
	private bool shouldPortraitShake(Dialogue d)
	{
		if (newPortaitShakeTimer > 0)
		{
			return true;
		}
		List<int> shakePortraits = d.speaker.GetData()?.ShakePortraits;
		if (shakePortraits != null && shakePortraits.Count > 0)
		{
			return shakePortraits.Contains(d.getPortraitIndex());
		}
		return false;
	}

	public void drawPortrait(SpriteBatch b)
	{
		NPC speaker = characterDialogue.speaker;
		if (!Game1.IsMasterGame && !speaker.EventActor)
		{
			GameLocation currentLocation = speaker.currentLocation;
			if (currentLocation == null || !currentLocation.IsActiveLocation())
			{
				NPC actualSpeaker = Game1.getCharacterFromName(speaker.Name);
				if (actualSpeaker != null && actualSpeaker.currentLocation.IsActiveLocation())
				{
					speaker = actualSpeaker;
				}
			}
		}
		if (width >= 642)
		{
			int xPositionOfPortraitArea = x + width - 448 + 4;
			int widthOfPortraitArea = x + width - xPositionOfPortraitArea;
			b.Draw(Game1.mouseCursors, new Rectangle(xPositionOfPortraitArea - 40, y, 36, height), new Rectangle(278, 324, 9, 1), Color.White);
			b.Draw(Game1.mouseCursors, new Vector2(xPositionOfPortraitArea - 40, y - 20), new Rectangle(278, 313, 10, 7), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			b.Draw(Game1.mouseCursors, new Vector2(xPositionOfPortraitArea - 40, y + height), new Rectangle(278, 328, 10, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			int portraitBoxX = xPositionOfPortraitArea + 76;
			int portraitBoxY = y + height / 2 - 148 - 36;
			b.Draw(Game1.mouseCursors, new Vector2(xPositionOfPortraitArea - 8, y), new Rectangle(583, 411, 115, 97), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			Texture2D portraitTexture = characterDialogue.overridePortrait ?? speaker.Portrait;
			Rectangle portraitSource = Game1.getSourceRectForStandardTileSheet(portraitTexture, characterDialogue.getPortraitIndex(), 64, 64);
			if (!portraitTexture.Bounds.Contains(portraitSource))
			{
				portraitSource = new Rectangle(0, 0, 64, 64);
			}
			int xOffset = (shouldPortraitShake(characterDialogue) ? Game1.random.Next(-1, 2) : 0);
			b.Draw(portraitTexture, new Vector2(portraitBoxX + 16 + xOffset, portraitBoxY + 24), portraitSource, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			SpriteText.drawStringHorizontallyCenteredAt(b, speaker.getName(), xPositionOfPortraitArea + widthOfPortraitArea / 2, portraitBoxY + 296 + 16);
			if (shouldDrawFriendshipJewel())
			{
				b.Draw(Game1.mouseCursors, new Vector2(friendshipJewel.X, friendshipJewel.Y), (Game1.player.getFriendshipHeartLevelForNPC(speaker.Name) >= 10) ? new Rectangle(269, 494, 11, 11) : new Rectangle(Math.Max(140, 140 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0 / 250.0) * 11), Math.Max(532, 532 + Game1.player.getFriendshipHeartLevelForNPC(speaker.Name) / 2 * 11), 11, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			}
		}
	}

	public string getCurrentString()
	{
		if (characterDialogue != null)
		{
			string s = ((characterDialoguesBrokenUp.Count <= 0) ? characterDialogue.getCurrentDialogue().Trim().Replace(Environment.NewLine, "") : characterDialoguesBrokenUp.Peek().Trim().Replace(Environment.NewLine, ""));
			if (!Game1.options.showPortraits)
			{
				s = characterDialogue.speaker.getName() + ": " + s;
			}
			return s;
		}
		if (dialogues.Count > 0)
		{
			return dialogues[0].Trim().Replace(Environment.NewLine, "");
		}
		return "";
	}

	public override void update(GameTime time)
	{
		base.update(time);
		if (Game1.options.SnappyMenus && !Game1.lastCursorMotionWasMouse)
		{
			Game1.mouseCursorTransparency = 0f;
		}
		else
		{
			Game1.mouseCursorTransparency = 1f;
		}
		if (isQuestion && characterIndexInDialogue >= getCurrentString().Length - 1 && !transitioning)
		{
			Game1.mouseCursorTransparency = 1f;
			if (!_showedOptions)
			{
				_showedOptions = true;
				if (responses != null)
				{
					responseCC = new List<ClickableComponent>();
					int responseY = y - (heightForQuestions - height) + SpriteText.getHeightOfString(getCurrentString(), width) + 48;
					for (int i = 0; i < responses.Length; i++)
					{
						responseCC.Add(new ClickableComponent(new Rectangle(x + 8, responseY, width - 8, SpriteText.getHeightOfString(responses[i].responseText, width) + 16), "")
						{
							myID = i,
							downNeighborID = ((i < responses.Length - 1) ? (i + 1) : (-1)),
							upNeighborID = ((i > 0) ? (i - 1) : (-1))
						});
						responseY += SpriteText.getHeightOfString(responses[i].responseText, width) + 16;
					}
				}
				populateClickableComponentList();
				if (Game1.options.gamepadControls)
				{
					snapToDefaultClickableComponent();
					selectedResponse = currentlySnappedComponent.myID;
				}
			}
		}
		if (safetyTimer > 0)
		{
			safetyTimer -= time.ElapsedGameTime.Milliseconds;
		}
		if (questionFinishPauseTimer > 0)
		{
			questionFinishPauseTimer -= time.ElapsedGameTime.Milliseconds;
			return;
		}
		if (transitioning)
		{
			if (!transitionInitialized)
			{
				transitionInitialized = true;
				transitionX = x + width / 2;
				transitionY = y + height / 2;
				transitionWidth = 0;
				transitionHeight = 0;
			}
			if (transitioningBigger)
			{
				int num = transitionWidth;
				transitionX -= (int)((float)time.ElapsedGameTime.Milliseconds * 3f);
				transitionY -= (int)((float)time.ElapsedGameTime.Milliseconds * 3f * ((float)(isQuestion ? heightForQuestions : height) / (float)width));
				transitionX = Math.Max(x, transitionX);
				transitionY = Math.Max(isQuestion ? (y + height - heightForQuestions) : y, transitionY);
				transitionWidth += (int)((float)time.ElapsedGameTime.Milliseconds * 3f * 2f);
				transitionHeight += (int)((float)time.ElapsedGameTime.Milliseconds * 3f * ((float)(isQuestion ? heightForQuestions : height) / (float)width) * 2f);
				transitionWidth = Math.Min(width, transitionWidth);
				transitionHeight = Math.Min(isQuestion ? heightForQuestions : height, transitionHeight);
				if (num == 0 && transitionWidth > 0)
				{
					playOpeningSound();
				}
				if (transitionX == x && transitionY == (isQuestion ? (y + height - heightForQuestions) : y))
				{
					transitioning = false;
					characterAdvanceTimer = 90;
					setUpIcons();
					transitionX = x;
					transitionY = y;
					transitionWidth = width;
					transitionHeight = height;
				}
			}
			else
			{
				transitionX += (int)((float)time.ElapsedGameTime.Milliseconds * 3f);
				transitionY += (int)((float)time.ElapsedGameTime.Milliseconds * 3f * ((float)height / (float)width));
				transitionX = Math.Min(x + width / 2, transitionX);
				transitionY = Math.Min(y + height / 2, transitionY);
				transitionWidth -= (int)((float)time.ElapsedGameTime.Milliseconds * 3f * 2f);
				transitionHeight -= (int)((float)time.ElapsedGameTime.Milliseconds * 3f * ((float)height / (float)width) * 2f);
				transitionWidth = Math.Max(0, transitionWidth);
				transitionHeight = Math.Max(0, transitionHeight);
				if (transitionWidth == 0 && transitionHeight == 0)
				{
					closeDialogue();
				}
			}
		}
		if (!transitioning && !showTyping && characterIndexInDialogue < getCurrentString().Length)
		{
			finishTyping();
		}
		if (!transitioning && characterIndexInDialogue < getCurrentString().Length)
		{
			characterAdvanceTimer -= time.ElapsedGameTime.Milliseconds;
			if (characterAdvanceTimer <= 0)
			{
				characterAdvanceTimer = 30;
				int old = characterIndexInDialogue;
				characterIndexInDialogue = Math.Min(characterIndexInDialogue + 1, getCurrentString().Length);
				if (characterIndexInDialogue != old && characterIndexInDialogue == getCurrentString().Length)
				{
					Game1.playSound("dialogueCharacterClose");
				}
				if (characterIndexInDialogue > 1 && characterIndexInDialogue < getCurrentString().Length && Game1.options.dialogueTyping)
				{
					Game1.playSound("dialogueCharacter");
				}
			}
		}
		if (!transitioning && dialogueIcon != null)
		{
			dialogueIcon.update(time);
		}
		if (!transitioning && newPortaitShakeTimer > 0)
		{
			newPortaitShakeTimer -= time.ElapsedGameTime.Milliseconds;
		}
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		width = 1200;
		height = 384;
		x = (int)Utility.getTopLeftPositionForCenteringOnScreen(width, height).X;
		y = Game1.uiViewport.Height - height - 64;
		friendshipJewel = new Rectangle(x + width - 64, y + 256, 44, 44);
		setUpIcons();
	}

	public override void draw(SpriteBatch b)
	{
		if (width < 16 || height < 16)
		{
			return;
		}
		if (transitioning)
		{
			drawBox(b, transitionX, transitionY, transitionWidth, transitionHeight);
			drawMouse(b);
			return;
		}
		if (isQuestion)
		{
			drawBox(b, x, y - (heightForQuestions - height), width, heightForQuestions);
			b.Draw(Game1.mouseCursors_1_6, new Vector2(x + width - 72, y - (heightForQuestions - height) - 88), new Rectangle(495, 461, 17, 19), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(Game1.mouseCursors_1_6, new Vector2(x + width - 52, y - (heightForQuestions - height) - 88 + 16), new Rectangle(470 + (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 900 / 150 * 7, 447, 7, 12), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			SpriteText.drawString(b, getCurrentString(), x + 8, y + 12 - (heightForQuestions - height), characterIndexInDialogue, width - 16);
			if (characterIndexInDialogue >= getCurrentString().Length - 1)
			{
				int responseY = y - (heightForQuestions - height) + SpriteText.getHeightOfString(getCurrentString(), width - 16) + 48;
				for (int i = 0; i < responses.Length; i++)
				{
					if (i == selectedResponse)
					{
						IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(375, 357, 3, 3), x + 4, responseY - 8, width - 8, SpriteText.getHeightOfString(responses[i].responseText, width) + 16, Color.White, 4f, drawShadow: false);
					}
					SpriteText.drawString(b, responses[i].responseText, x + 8, responseY, 999999, width, 999999, (selectedResponse == i) ? 1f : 0.6f);
					responseY += SpriteText.getHeightOfString(responses[i].responseText, width) + 16;
				}
			}
		}
		else
		{
			drawBox(b, x, y, width, height);
			if (!isPortraitBox() && !isQuestion)
			{
				SpriteText.drawString(b, getCurrentString(), x + 8, y + 8, characterIndexInDialogue, width);
			}
		}
		if (isPortraitBox() && !isQuestion)
		{
			drawPortrait(b);
			if (!isQuestion)
			{
				SpriteText.drawString(b, getCurrentString(), x + 8, y + 8, characterIndexInDialogue, width - 460 - 24);
			}
		}
		if (dialogueIcon != null && characterIndexInDialogue >= getCurrentString().Length - 1)
		{
			dialogueIcon.draw(b, localPosition: true);
		}
		if (aboveDialogueImage != null)
		{
			drawBox(b, x + width / 2 - (int)((float)(aboveDialogueImage.sourceRect.Width / 2) * aboveDialogueImage.scale), y - 64 - 4 - (int)((float)aboveDialogueImage.sourceRect.Height * aboveDialogueImage.scale), (int)((float)aboveDialogueImage.sourceRect.Width * aboveDialogueImage.scale), (int)((float)aboveDialogueImage.sourceRect.Height * aboveDialogueImage.scale) + 8);
			Utility.drawWithShadow(b, aboveDialogueImage.texture, new Vector2((float)(x + width / 2) - (float)(aboveDialogueImage.sourceRect.Width / 2) * aboveDialogueImage.scale, y - 64 - (int)((float)aboveDialogueImage.sourceRect.Height * aboveDialogueImage.scale)), aboveDialogueImage.sourceRect, Color.White, 0f, Vector2.Zero, aboveDialogueImage.scale, flipped: false, 1f);
		}
		if (hoverText.Length > 0)
		{
			SpriteText.drawStringWithScrollBackground(b, hoverText, friendshipJewel.Center.X - SpriteText.getWidthOfString(hoverText) / 2, friendshipJewel.Y - 64);
		}
		drawMouse(b);
	}
}
