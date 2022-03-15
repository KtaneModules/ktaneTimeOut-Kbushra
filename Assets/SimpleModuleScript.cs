using System.Collections.Generic;
using UnityEngine;
using KModkit;
using Newtonsoft.Json;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections;
using System;
using Rnd = UnityEngine.Random;

public class SimpleModuleScript : MonoBehaviour {

	public KMAudio audio;
	public KMBombInfo info;
	public KMBombModule module;
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
		Debug.LogFormat("[Timed Out #{0}] Stage 1's int is {1}", ModuleId, stage1Int);

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
		Debug.LogFormat("[Timed Out #{0}] Stage 2's int is {1}", ModuleId, stage2Int);

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
		Debug.LogFormat("[Timed Out #{0}] Stage 3's left and right int is {1} and {2}", ModuleId, stage3LeftInt, stage3RightInt);

		//stage 4
		stage4Int = (stage1Int + stage2Int + stage3Int) % 10;
		Debug.LogFormat("[Timed Out #{0}] Stage 4's int is {1}", ModuleId, stage4Int);

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
			Log ("Next stage.");
			stage3half = 99;
		}
	}

	void clock(KMSelectable pressedButton)
	{
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		int buttonPosition = Array.IndexOf(watch, pressedButton);

		if (_isSolved == false) 
		{
			switch (buttonPosition) 
			{
			case 0:
				if (stage == 1) 
				{
					if (time % 10 == stage1Int)
					{
						stage++;
						pressed = true;
						Log ("Next stage.");
					} 
					else 
					{
						incorrect = true;
						Debug.LogFormat("[Timed Out #{0}] Wrong! Right time to press is {1} and you pressed at {2}", ModuleId, stage1Int, time % 10);
					}
				}
				if (stage == 2 && pressed != true)
				{
					if (time % 10 == stage2Int) 
					{
						stage++;
						pressed = true;
						Log ("Next stage.");
					} 
					else 
					{
						incorrect = true;
						Debug.LogFormat("[Timed Out #{0}] Wrong! Right time to press is {1} and you pressed at {2}", ModuleId, stage2Int, time % 10);
					}
				}
				if (stage == 3 && pressed != true) 
				{
					incorrect = true;
				}
				if (stage == 4 && pressed != true) 
				{
					if (time % 10 == stage4Int) 
					{
						stage++;
						pressed = true;
						Log ("Next stage.");
					}
					else
					{
						incorrect = true;
						Debug.LogFormat("[Timed Out #{0}] Wrong! Right time to press is {1} and you pressed at {2}", ModuleId, stage4Int, time % 10);
					}
				}
				if (stage == 5 && pressed != true)
				{
					module.HandlePass ();
					Log ("Solved.");
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
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		int buttonPosition = Array.IndexOf(buttons, pressedButton);

		if (_isSolved == false)
		{
			switch (buttonPosition) 
			{
			case 0:
				if (stage == 3) 
				{
					if (stage3half == 0) 
					{
						if (time % 60 == stage3LeftInt) 
						{
							stage3half++;
							Log ("Button has been pressed.");
							pressed = true;
						}
						else
						{
							incorrect = true;
							Debug.LogFormat("[Timed Out #{0}] Wrong! Right time to press is {1} and you pressed at {2}", ModuleId, stage3LeftInt, time % 60);
						}
					}
					else
					{
						incorrect = true;
						Log ("Wrong order");
					}
				}
				else
				{
					incorrect = true;
					Log ("Not stage 3");
				}
				break;
			case 1:
				if (stage == 3) 
				{
					if (stage3half == 1 && pressed != true) 
					{
						if (time % 60 == stage3RightInt)
						{
							stage3half++;
							Log ("Button has been pressed.");
							pressed = true;
						}
						else 
						{
							incorrect = true;
							Debug.LogFormat("[Timed Out #{0}] Wrong! Right time to press is {1} and you pressed at {2}", ModuleId, stage3RightInt, time % 60);
						}
					}
					else 
					{
						incorrect = true;
						Log ("Wrong order");
					}
				}
				else
				{
					incorrect = true;
					Log ("Not stage 3");
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
}
