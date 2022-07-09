using UnityEngine;
using KModkit;
using System.Linq;
using System.Collections;
using System;
using Rnd = UnityEngine.Random;

public class SimpleModuleScript : MonoBehaviour {

	public KMAudio audio;
	public KMBombInfo info;
	public KMBombModule module;
	public KMSelectable modSelectable;
	public KMSelectable[] watch;
	public KMSelectable[] buttons;
	static int ModuleIdCounter = 1;
	int ModuleId;

	public Transform[] mover;
	public Transform[] target;

	bool _isSolved = false;
	bool incorrect = false;

	private static readonly int[,] stage1Table = new int[6, 8] 
	{
		{9, 7, 4, 2, 3, 5, 6, 1},
		{2, 3, 5, 9, 0, 8, 4, 1},
		{5, 3, 0, 7, 4, 1, 8, 9},
		{5, 6, 9, 2, 3, 8, 1, 0},
		{2, 4, 5, 6, 8, 7, 2, 1},
		{1, 3, 6, 4, 9, 0, 2, 1}
	};
	private int stage1TableRow = 0;
	private int stage1TableColumn = 0;

	public TextMesh watchTextMiddle;
	public TextMesh watchTextLeft;
	public TextMesh watchTextRight;

	public int stage1Int = 0;
	public int stage2Int = 0;
	private int stage2RandInt = 0;
	public int stage2ModifyInt = 0;
	private int stage3Int = 0;
	private int stage3RandLeftInt = 0;
	public int stage3LeftInt = 0;
	private int stage3RandRightInt = 0;
	public int stage3RightInt = 0;
	public int stage4Int = 0;

	public int stage = 1;
	public int stage3half = 0;

	bool pressed;

	private int time;


	void Awake() 
	{
		ModuleId = ModuleIdCounter++;

		foreach (KMSelectable button in watch)
		{
			KMSelectable pressedButton = button;
			button.OnInteract += delegate () { clock(pressedButton); return false; };
		}

		foreach (KMSelectable button in buttons)
		{
			KMSelectable pressedButton = button;
			button.OnInteract += delegate () { buttonPress(pressedButton); return false; };
		}
	}

	void Start ()
	{
		watchTextMiddle.text = "?";

		watchTextLeft.text = "?";

		watchTextRight.text = "?";

		stageCreator ();
	}

	void stageCreator()
	{
		//stage 1
		if (info.GetSerialNumberNumbers ().LastOrDefault () < 5)
		{
			stage1TableRow = info.GetSerialNumberNumbers ().LastOrDefault ();
		}
		else
		{
			stage1TableRow = 5;
		}

		if (info.GetBatteryCount () % 2 == 0)
		{
			stage1TableColumn = info.GetBatteryCount () * 2;

			if (stage1TableColumn > 6)
			{
				stage1TableColumn = 7;
			}
		}
		else
		{
			stage1TableColumn = info.GetPortCount ();

			if (stage1TableColumn > 6)
			{
				stage1TableColumn = 7;
			}
		}
		stage1Int = stage1Table [stage1TableRow, stage1TableColumn];
		Debug.LogFormat("[Timed Out #{0}] Stage 1's row is {1}", ModuleId, stage1TableRow);
		Debug.LogFormat("[Timed Out #{0}] Stage 1's column is {1}", ModuleId, stage1TableColumn);
		Debug.LogFormat("[Timed Out #{0}] Stage 1's digit is {1}", ModuleId, stage1Int);

		//stage 2
		stage2RandInt = Rnd.Range(100,1000);

		stage2ModifyInt = stage2RandInt * 79;
		stage2ModifyInt = stage2ModifyInt % 100;

		if (stage2ModifyInt == 1 || stage2ModifyInt == 2 || stage2ModifyInt == 4 | stage2ModifyInt == 5 || stage2ModifyInt == 10 || stage2ModifyInt == 20 || stage2ModifyInt == 25 || stage2ModifyInt == 50 || stage2ModifyInt == 100) 
		{
			stage2Int = stage1Int;
		}
		else
		{
			stage2Int = (stage2ModifyInt + info.GetPortCount ()) % 10;
		}
		Debug.LogFormat("[Timed Out #{0}] Stage 2's shown number is {1}", ModuleId, stage2RandInt);
		Debug.LogFormat("[Timed Out #{0}] Stage 2's digit is {1}", ModuleId, stage2Int);

		//stage 3
		stage3RandLeftInt = Rnd.Range (1, 10);
		stage3RandRightInt = Rnd.Range (1, 10);

		if (info.GetPortCount () != 0)
		{
			stage3LeftInt = stage3RandLeftInt / info.GetPortCount ();
		}
		else
		{
			stage3LeftInt = stage3RandLeftInt / 1;
		}
		stage3LeftInt++;
		stage3LeftInt = stage3LeftInt * 7;
		if (info.GetBatteryCount () != 0) 
		{
			stage3LeftInt = stage3LeftInt % info.GetBatteryCount ();
		}
		else
		{
			stage3LeftInt = stage3LeftInt % 1;
		}
		stage3LeftInt++;
		stage3LeftInt = stage3LeftInt % 60;

		stage3RightInt = stage3RandRightInt * info.GetPortCount ();
		stage3RightInt = stage3RightInt + 10;
		if (info.GetIndicators ().ToList ().Count != 0)
		{
			stage3RightInt = stage3RightInt / info.GetIndicators ().ToList ().Count;
		}
		else
		{
			stage3RightInt = stage3RightInt / 1;
		}
		stage3RightInt = stage3RightInt * 2;
		stage3RightInt = stage3RightInt % 3;
		stage3RightInt = stage3RightInt + 35;
		stage3RightInt = stage3RightInt % 60;

		stage3Int = stage3LeftInt + stage3RightInt;
		Debug.LogFormat("[Timed Out #{0}] Stage 3's shown digits are {1} and {2}", ModuleId, stage3RandLeftInt, stage3RandRightInt);
		Debug.LogFormat("[Timed Out #{0}] Stage 3's left and right numbers are {1} and {2}", ModuleId, stage3LeftInt, stage3RightInt);

		//stage 4
		stage4Int = (stage1Int + stage2Int + stage3Int) % 10;
		Debug.LogFormat("[Timed Out #{0}] Stage 4's digit is {1}", ModuleId, stage4Int);

		//No stage 5 working out so YAYA!
	}

	void Update()
	{
		time = (int)info.GetTime ();
		if (stage == 1)
		{
			watchTextLeft.text = "?";
			watchTextRight.text = "?";
			watchTextMiddle.text = "?";
		}
		else
		{
			if (stage == 2) 
			{
				watchTextMiddle.text = stage2RandInt.ToString ();
			}
			else
			{
				watchTextMiddle.text = "";
			}

			if (stage == 3)
			{
				watchTextLeft.text = stage3RandLeftInt.ToString ();
				watchTextRight.text = stage3RandRightInt.ToString ();
				MoveDaButtons();
			}
			else
			{
				watchTextLeft.text = "";
				watchTextRight.text = "";
			}
		}


		if (stage3half == 2 && pressed != true)
		{
			stage++;
			Log("Correct! Moving to stage 4.");
			stage3half = 99;
		}
	}

	void clock(KMSelectable pressedButton)
	{
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressedButton.transform);
		int buttonPosition = Array.IndexOf(watch, pressedButton);

		if (_isSolved == false) 
		{
			Debug.LogFormat("[Timed Out #{0}] The watch was pressed at {1}", ModuleId, info.GetFormattedTime());
			switch (buttonPosition) 
			{
			case 0:
				if (stage == 1) 
				{
					if (time % 10 == stage1Int)
					{
						stage++;
						pressed = true;
						Log("Correct! Moving to stage 2.");
					} 
					else 
					{
						incorrect = true;
						Log("Wrong! The module gave a strike because the last digit of the bomb timer was not stage 1's digit.");
					}
				}
				if (stage == 2 && pressed != true)
				{
					if (time % 10 == stage2Int) 
					{
						stage++;
						pressed = true;
						Log("Correct! Moving to stage 3.");
					} 
					else 
					{
						incorrect = true;
						Log("Wrong! The module gave a strike because the last digit of the bomb timer was not stage 2's digit.");
					}
				}
				if (stage == 3 && pressed != true) 
				{
					incorrect = true;
					if (stage3half == 0)
						Log("Wrong! The module gave a strike because the left button should have been pressed.");
					else
						Log("Wrong! The module gave a strike because the right button should have been pressed.");
				}
				if (stage == 4 && pressed != true) 
				{
					if (time % 10 == stage4Int) 
					{
						stage++;
						pressed = true;
						Log("Correct! Moving to stage 5.");
					}
					else
					{
						incorrect = true;
						Log("Wrong! The module gave a strike because the last digit of the bomb timer was not stage 4's digit.");
					}
				}
				if (stage == 5 && pressed != true)
				{
					module.HandlePass ();
					Log("Module solved.");
					_isSolved = true;
					pressed = true;
				}
				break;
			}
			if (incorrect)
			{
				module.HandleStrike ();
				incorrect = false;
			}
			else 
			{
				//This will be decided in the button press itself
			}
			pressed = false;
		}
	}

	void buttonPress(KMSelectable pressedButton)
	{
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressedButton.transform);
		int buttonPosition = Array.IndexOf(buttons, pressedButton);

		if (_isSolved == false)
		{
			switch (buttonPosition) 
			{
			case 0:
				Debug.LogFormat("[Timed Out #{0}] The left button was pressed at {1}", ModuleId, info.GetFormattedTime());
				if (stage == 3) 
				{
					if (stage3half == 0) 
					{
						if (time % 60 == stage3LeftInt) 
						{
							stage3half++;
							pressed = true;
						}
						else
						{
							incorrect = true;
							Log("Wrong! The module gave a strike because the seconds digits of the bomb timer were not stage 4's left number.");
						}
					}
					else
					{
						incorrect = true;
						Log("Wrong! The module gave a strike because the right button should have been pressed.");
					}
				}
				else
				{
					incorrect = true;
					Log("Wrong! The module gave a strike because it is no longer stage 3.");
				}
				break;
			case 1:
				Debug.LogFormat("[Timed Out #{0}] The right button was pressed at {1}", ModuleId, info.GetFormattedTime());
				if (stage == 3) 
				{
					if (stage3half == 1 && pressed != true) 
					{
						if (time % 60 == stage3RightInt)
						{
							stage3half++;
							pressed = true;
						}
						else 
						{
							incorrect = true;
							Log("Wrong! The module gave a strike because the seconds digits of the bomb timer were not stage 4's right number.");
						}
					}
					else 
					{
						incorrect = true;
						Log("Wrong! The module gave a strike because the left button should have been pressed.");
					}
				}
				else
				{
					incorrect = true;
					Log("Wrong! The module gave a strike because it is no longer stage 3.");
				}
				break;
			}
			if (incorrect) 
			{
				module.HandleStrike ();
				incorrect = false;
			}
			else 
			{
				//This will be decided in the button press itself
			}
			pressed = false;
		}
	}



	void Log(string message)
	{
		Debug.LogFormat("[Timed Out #{0}] {1}", ModuleId, message);
	}

	void MoveDaButtons()
	{
		mover[0].position = target[0].position;
		mover[1].position = target[1].position;
	}

	//Twitch Plays support

	//The help message with all commands we want users to be able to use.
	//The pragma statements disable your code environment from yelling at you for the help message being unused in the code.
	#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} watch (#) [Presses the watch (optionally when the last digit of the bomb timer is '#')] | !{0} left/right <##> [Presses the left or right button when the seconds digits are '##']";
	#pragma warning restore 414

	//The command processor, all commands for this mod come through here.
	//All commands normally follow the format of !<id> <command> but !<id> is not included in the string command here.
	IEnumerator ProcessTwitchCommand(string command)
	{
		//Splits the command by spaces into a string array.
		string[] parameters = command.Split(' ');
		//If the string array has more than two elements, then it cannot possibly match our expected commands.
		if (parameters.Length > 2)
        {
			yield return "sendtochaterror Too many parameters!"; //sendtochaterror is used to output an error message to the chat.
			yield break; //Stop code here since we know the command is invalid.
        }
		//If the first element of the array is "watch" then do the following.
		if (parameters[0].EqualsIgnoreCase("watch"))
        {
			//If the string array has one element then the command was just "watch", so just press the watch.
			if (parameters.Length == 1)
            {
				yield return null; //If nothing is yield returned then TP assumes the command was invalid and sends out its own error message, we don't want this so let's return null.
				watch[0].OnInteract();
			}
			else
            {
				//Makes sure the second element in the string array is a digit.
				int digit = -1;
				if (!int.TryParse(parameters[1], out digit)) //Trys to throw the string into an int but if it cannot it will return false.
                {
					yield return "sendtochaterror The specified digit is invalid!";
					yield break;
                }
				if (digit < 0 || digit > 9)
				{
					yield return "sendtochaterror The specified digit is invalid!";
					yield break;
				}
				yield return null;
				//Waits for the last digit of the timer to be what was specified and allows for !cancel to stop it.
				while (time % 10 != digit) yield return "trycancel Halted waiting to press the watch due to a cancel request!";
				watch[0].OnInteract();
            }
        }
		//Otherwise, if the first element of the array is "left" then do the following.
		else if (parameters[0].EqualsIgnoreCase("left"))
		{
			//Check if the command was just "left", which is not a valid command.
			if (parameters.Length == 1)
				yield return "sendtochaterror Please specify the seconds digits to press the left button on!";
			else
			{
				//Makes sure the second element in the string array is a two digit number from 00-59.
				int digits = -1;
				if (!int.TryParse(parameters[1], out digits))
				{
					yield return "sendtochaterror The specified seconds digits are invalid!";
					yield break;
				}
				if (digits < 0 || digits > 59 || parameters[1].Length != 2) //We have to check for the length here because otherwise single digits would be accepted.
                {
					yield return "sendtochaterror The specified seconds digits are invalid!";
					yield break;
				}
				yield return null;
				//Waits for the seconds digits of the timer to be what was specified and allows for !cancel to stop it.
				while (time % 60 != digits) yield return "trycancel Halted waiting to press the left button due to a cancel request!";
				buttons[0].OnInteract();
			}
		}
		//Otherwise, if the first element of the array is "right" then do the following.
		else if (parameters[0].EqualsIgnoreCase("right"))
		{
			//Does the same as what was done for the left command, but replaces all error messages that have "left" with "right" and presses the right button.
			if (parameters.Length == 1)
				yield return "sendtochaterror Please specify the seconds digits to press the right button on!";
			else
			{
				int digits = -1;
				if (!int.TryParse(parameters[1], out digits))
				{
					yield return "sendtochaterror The specified seconds digits are invalid!";
					yield break;
				}
				if (digits < 0 || digits > 59 || parameters[1].Length != 2)
				{
					yield return "sendtochaterror The specified seconds digits are invalid!";
					yield break;
				}
				yield return null;
				while (time % 60 != digits) yield return "trycancel Halted waiting to press the right button due to a cancel request!";
				buttons[1].OnInteract();
			}
		}
	}

	//The autosolve processor, which handles solving the module when the module is voted to be autosolved, an exception occurs, or the good team wins versus mode.
	//This is an optional method, as TP can just turn the status light green, but who doesn't like seeing a mod solve itself?
	IEnumerator TwitchHandleForcedSolve()
    {
		//If it's stage 1, wait for the correct time and press the watch.
		if (stage == 1)
        {
			while (time % 10 != stage1Int) yield return true; //Using yield return true in this method tells TP to deal with autosolving other mods while waiting for the condition to be true.
			watch[0].OnInteract();
			yield return new WaitForSeconds(.1f); //I like to include a little delay after each press in my solvers (even though it would be rare to see in action cause you wait a lot with this mod).
        }
		//If it's stage 2, wait for the correct time and press the watch.
		if (stage == 2)
		{
			while (time % 10 != stage2Int) yield return true;
			watch[0].OnInteract();
			yield return new WaitForSeconds(.1f);
		}
		//If it's stage 3, press the left button if it hasn't been pressed yet, and then the right one.
		if (stage == 3)
		{
			if (stage3half == 0)
            {
				while (time % 60 != stage3LeftInt) yield return true;
				buttons[0].OnInteract();
				yield return new WaitForSeconds(.1f);
			}
			while (time % 60 != stage3RightInt) yield return true;
			buttons[1].OnInteract();
			yield return new WaitForSeconds(.1f);
		}
		//If it's stage 4, wait for the correct time and press the watch.
		if (stage == 4)
		{
			while (time % 10 != stage4Int) yield return true;
			watch[0].OnInteract();
			yield return new WaitForSeconds(.1f);
		}
		//If the mod isn't solved by this point it is stage 5, so press the watch.
		watch[0].OnInteract();
	}
}