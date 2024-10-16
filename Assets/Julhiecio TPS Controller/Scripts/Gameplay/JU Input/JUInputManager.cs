using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using JUTPS.JUInputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem;

using JUTPS.CrossPlataform;

namespace JUTPS.JUInputSystem
{

	[AddComponentMenu("JU TPS/Input/JU Input Manager")]

	public class JUInputManager : MonoBehaviour
	{

		/* 	필드 지연 초기화
			이 코드에서 _inputs와 InputActions의 이름을 다르게 쓴 이유는 코드의 명확성 및 구조적인 이유 때문일 가능성이 높습니다.
			_inputs는 클래스 내부에서만 사용하는 변수로, 외부에서 직접 접근할 수 없도록 private으로 선언되어 있습니다. 일반적으로 C#에서는 필드 이름에 언더스코어(_)를 붙여서 클래스 내에서 관리되는 변수라는 것을 명확히 하는 관례가 있습니다.
			InputActions는 외부에서 접근 가능한 프로퍼티입니다. 프로퍼티는 클래스 외부에서 해당 필드에 접근할 수 있는 인터페이스 역할을 합니다. 이 프로퍼티는 내부에서 _inputs 필드를 반환하는 로직을 가지고 있으며, 필요할 때만 _inputs를 초기화하고 설정하는 방식입니다.

			이 부분은 JUTPSInputControlls라는 클래스를 기반으로 _inputs라는 private 필드를 선언한 것입니다. 이 필드는 클래스 내에서만 사용되며, 아직 초기화되지는 않은 상태입니다. 나중에 InputActions 프로퍼티에서 이 필드가 지연 초기화(lazy initialization) 방식으로 필요할 때만 생성되고 사용할 수 있도록 관리됩니다.

			정리하면:
			_inputs는 JUTPSInputControlls 클래스의 인스턴스를 저장하는 private 변수입니다.
			처음에는 null 값으로 존재하다가, InputActions 프로퍼티에서 최초로 접근할 때 new JUTPSInputControlls()로 인스턴스화됩니다.
			이를 통해 외부에서 InputActions 프로퍼티를 통해 JUTPSInputControlls 객체에 접근할 수 있게 되는 구조입니다.

			이름이 너무 비슷하면 혼동이 올 수 있기 때문에, 필드와 프로퍼티의 역할을 분명히 하기 위해 일부러 다른 이름을 사용했을 가능성이 있습니다. 특히, 이런 방식은 더 큰 프로젝트에서 유지보수성을 높이는 데 도움이 될 수 있습니다.
			말씀하신 대로 일반적으로는 클래스 이름을 기반으로 한 프로퍼티 이름(InputActions)과 필드 이름(_inputActions)을 대소문자 차이만으로 맞추는 것이 흔한 패턴이므로, 이 프로젝트에서는 코딩 스타일이 약간 다르게 적용된 것으로 보입니다.
		*/
		private JUTPSInputControlls _inputs; 
		public JUTPSInputControlls InputActions
		{
			get
			{
				if (_inputs == null)
				{
					_inputs = new JUTPSInputControlls();
					_inputs.Enable();
					AddInputUpListeners(InputActions.Player);
				}

				return _inputs;
			}
		}

		private bool BlockStandardInputs;
		public bool IsBlockingDefaultInputs { get => BlockStandardInputs; }


		/// <summary>
		/// When calling this function it blocks the default JU Input Manager inputs, useful if you want to rewrite all input controls
		/// for example the MobileRig Script.
		/// </summary>
		public void EnableBlockStandardInputs() { BlockStandardInputs = true; }

		/// <summary>
		/// When calling this function, it disables blocking for the default JU Input Manager inputs.
		/// use EnableBlockStandardInputs to enable blocking default inputs.
		/// </summary>
		public void DisableBlockStandardInputs() { BlockStandardInputs = false; }

		//Move and Rotate Axis
		[HideInInspector] public float MoveHorizontal;
		[HideInInspector] public float MoveVertical;
		[HideInInspector] public float RotateHorizontal;
		[HideInInspector] public float RotateVertical;

		//>>> Input Bools
		//OnPressing
		[HideInInspector]
		public bool PressedShooting, PressedAiming, PressedReload, PressedRun, PressedJump, PressedPunch,
			PressedCrouch, PressedProne, PressedRoll, PressedPickup, PressedInteract, PressedNextItem, PressedPreviousItem;
		//OnDown
		[HideInInspector]
		public bool PressedShootingDown, PressedAimingDown, PressedReloadDown, PressedRunDown, PressedJumpDown, PressedPunchDown,
			PressedCrouchDown, PressedProneDown, PressedRollDown, PressedPickupDown, PressedInteractDown, PressedNextItemDown, PressedPreviousItemDown, PressedOpenInventoryDown;
		//OnUp
		[HideInInspector]
		public bool PressedShootingUp, PressedAimingUp, PressedReloadUp, PressedRunUp, PressedJumpUp, PressedPunchUp,
			PressedCrouchUp, PressedProneUp, PressedRollUp, PressedPickupUp, PressedInteractUp, PressedNextItemUp, PressedPreviousItemUp;



		public CustomTouchButton[] CustomTouchButton;
		public CustomTouchfield[] CustomTouchfield;
		public CustomJoystickVirtual[] CustomJoystickVirtual;
		[Header("(Old Input System)")]
		public CustomInputButton[] CustomButton;

		public static bool IsUsingGamepad;
		private void Update()
		{
			double gamepad = Gamepad.current != null ? Gamepad.current.lastUpdateTime : 0;
			//If are mouse and keyboard conected |                  |if mouse last update are lower than keyboard last update   >        value = KeyboardLastUpdate    :else  >  value = MouseLastUpdate

			// 삼항 연산자 --> 조건식 ? 참일 때의 값 : 거짓일 때의 값
			double keyboardAndMouseLastUsed = (Keyboard.current != null && Mouse.current != null) ? ((Mouse.current.lastUpdateTime < Keyboard.current.lastUpdateTime) ? Keyboard.current.lastUpdateTime : Mouse.current.lastUpdateTime) : 0;
			IsUsingGamepad = (gamepad > keyboardAndMouseLastUsed) ? true : false;

			if (BlockStandardInputs) return;

			UpdateGetButtonDown();
			UpdateGetButton();

			//In the new input system the "GetUp" method are now events (see the method "AddInputUpListeners" bellow)
			//UpdateGetButtonUp();

			UpdateAxis();
		}


		// 위의 JUTPSInputControlls 프로퍼티 부분에서 사용되는 함수 
		// UpdateGetButtonDown() 이나 UpdateGetButton() 은 void Update() 에 넣었는데 이 Up 관련된 건 프로퍼티에 넣어놓음 
		// 이벤트 기반 처리: 입력 상태 변화가 있을 때만 특정 이벤트가 발생하여 처리하는 방식입니다. 이 방식은 Update() 메서드에 넣을 필요가 없으며, 입력 상태 변화에 즉각적으로 반응할 수 있습니다.
		/* ctx 
		ctx는 이벤트 핸들러의 매개변수로, InputAction.CallbackContext 타입을 가집니다. 이 매개변수는 입력 동작이 수행될 때 그 상태에 대한 정보를 제공합니다. 예를 들어, 입력이 눌렸는지, 놓였는지, 또는 얼마나 세게 눌렸는지 같은 정보들을 포함하고 있습니다.
		Unity의 새로운 입력 시스템에서 performed, started, canceled 같은 이벤트에 등록할 때, ctx는 해당 입력의 상태나 정보를 확인하는 데 사용되는 중요한 매개변수입니다.
		*/
		private void AddInputUpListeners(JUTPSInputControlls.PlayerActions input)
		{
			input.Run.performed += ctx => { PressedRunUp = false; }; // "Run" 액션이 수행되었을 때 PressedRunUp 변수를 false로 설정
			input.Run.canceled += ctx => { PressedRunUp = true; }; // "Run" 액션이 취소되었을 때 PressedRunUp 변수를 true로 설정

			input.Roll.performed += ctx => { PressedRollUp = false; };
			input.Roll.canceled += ctx => { PressedRollUp = true; };

			input.Jump.performed += ctx => { PressedJumpUp = false; };
			input.Jump.canceled += ctx => { PressedJumpUp = true; };

			input.Punch.performed += ctx => { PressedPunchUp = false; };
			input.Punch.canceled += ctx => { PressedPunchUp = true; };

			input.Crouch.performed += ctx => { PressedCrouchUp = false; };
			input.Crouch.canceled += ctx => { PressedCrouchUp = true; };

			input.Prone.performed += ctx => { PressedProneUp = false; };
			input.Prone.canceled += ctx => { PressedProneUp = true; };

			input.Fire.performed += ctx => { PressedShootingUp = false; };
			input.Fire.canceled += ctx => { PressedShootingUp = true; };

			input.Aim.performed += ctx => { PressedAimingUp = false; };
			input.Aim.canceled += ctx => { PressedAimingUp = true; };

			input.Reload.performed += ctx => { PressedReloadUp = false; };
			input.Reload.canceled += ctx => { PressedReloadUp = true; };

			input.Pickup.performed += ctx => { PressedPickupUp = false; };
			input.Pickup.canceled += ctx => { PressedPickupUp = true; };

			input.Interact.performed += ctx => { PressedInteractUp = false; };
			input.Interact.canceled += ctx => { PressedInteractUp = true; };

			input.Next.performed += ctx => { PressedNextItemUp = false; };
			input.Next.canceled += ctx => { PressedNextItemUp = true; };

			input.Previous.performed += ctx => { PressedPreviousItemUp = false; };
			input.Previous.canceled += ctx => { PressedPreviousItemUp = true; };
		}
		
		protected virtual void UpdateGetButtonDown()
		{
			// triggered는 입력 액션이 해당 프레임에서 발생했을 때만 true를 반환합니다.
			// 예를 들어, 점프 액션에 매핑된 버튼을 누르면 그 특정 프레임에서만 InputAction.triggered가 true를 반환하고, 다른 프레임에서는 false를 반환합니다.
			PressedJumpDown = InputActions.Player.Jump.triggered;
			PressedRunDown = InputActions.Player.Run.triggered;
			PressedPunchDown = InputActions.Player.Punch.triggered;
			PressedRollDown = InputActions.Player.Roll.triggered;
			PressedProneDown = InputActions.Player.Prone.triggered;
			PressedCrouchDown = InputActions.Player.Crouch.triggered;

			PressedShootingDown = InputActions.Player.Fire.triggered;
			PressedAimingDown = InputActions.Player.Aim.triggered;
			PressedReloadDown = InputActions.Player.Reload.triggered;

			PressedPickupDown = InputActions.Player.Pickup.triggered;
			PressedInteractDown = InputActions.Player.Interact.triggered;
			PressedNextItemDown = InputActions.Player.Next.triggered;
			PressedPreviousItemDown = InputActions.Player.Previous.triggered;
			PressedOpenInventoryDown = InputActions.Player.OpenInventory.triggered;
		}
		protected virtual void UpdateGetButton()
		{
			PressedJump = InputActions.Player.Jump.ReadValue<float>() == 1;
			PressedRun = InputActions.Player.Run.ReadValue<float>() == 1;
			PressedPunch = InputActions.Player.Punch.ReadValue<float>() == 1;
			PressedRoll = InputActions.Player.Roll.ReadValue<float>() == 1;
			PressedProne = InputActions.Player.Prone.ReadValue<float>() == 1;
			PressedCrouch = InputActions.Player.Crouch.ReadValue<float>() == 1;

			PressedShooting = InputActions.Player.Fire.ReadValue<float>() == 1;
			PressedAiming = InputActions.Player.Aim.ReadValue<float>() == 1;
			PressedReload = InputActions.Player.Reload.ReadValue<float>() == 1;

			PressedPickup = InputActions.Player.Pickup.ReadValue<float>() == 1;
			PressedInteract = InputActions.Player.Interact.ReadValue<float>() == 1;
			PressedNextItem = InputActions.Player.Next.ReadValue<float>() == 1;
			PressedPreviousItem = InputActions.Player.Previous.ReadValue<float>() == 1;

			//if(PressedProneDown) Debug.Log("hold to prone");
		}

		protected virtual void UpdateAxis()
		{

			// >>> Joystick Movements
			MoveHorizontal = InputActions.Player.Move.ReadValue<Vector2>().x;
			MoveVertical = InputActions.Player.Move.ReadValue<Vector2>().y;

			MoveHorizontal = Mathf.Clamp(MoveHorizontal, -1, 1);
			MoveVertical = Mathf.Clamp(MoveVertical, -1, 1);


			if (JUTPS.JUGameManager.IsMobileControls)
			{
				if (IsBlockingDefaultInputs) Debug.LogWarning("In the Game Manager the ''IsMobile'' variable is set to true, but there is no script blocking the default inputs. Add a Mobile Rig from the prefabs folder or create one.");
			}
			else
			{
				RotateHorizontal = InputActions.Player.Look.ReadValue<Vector2>().x;
				RotateVertical = InputActions.Player.Look.ReadValue<Vector2>().y;

			}
		}
	}




	[System.Serializable]
	public class CustomInputButton
	{
		public string Name;
		[SerializeField] private KeyCode Input = KeyCode.P;
		public bool Pressed()
		{
			return UnityEngine.Input.GetKey(Input);
		}
		public bool PressedDown()
		{
			return UnityEngine.Input.GetKeyDown(Input);
		}
		public bool PressedUp()
		{
			return UnityEngine.Input.GetKeyUp(Input);
		}
	}

	[System.Serializable]
	public class CustomTouchButton
	{
		public string Name;
		[SerializeField] private ButtonVirtual ButtonInput = null;
		public bool Pressed()
		{
			return ButtonInput.IsPressed;
		}

		public bool PressedDown()
		{
			return ButtonInput.IsPressedDown;
		}

		public bool PressedUp()
		{
			return ButtonInput.IsPressedUp;
		}
	}

	[System.Serializable]
	public class CustomTouchfield
	{
		public string Name;
		[SerializeField] private Touchfield TouchfieldInput = null;
		public Vector2 TouchDistance()
		{
			return TouchfieldInput.TouchDistance;
		}
	}
	[System.Serializable]
	public class CustomJoystickVirtual
	{
		public string Name;
		[SerializeField] private JoystickVirtual Joystick = null;
		public Vector2 JoystickInput()
		{
			return Joystick.InputVector;
		}
	}
	public class JUInput
	{
		private static JUInputManager JUInputInstance;
		public static JUInputManager Instance()
		{
			if (JUInputInstance == null)
			{
				GetJUInputInstance();
			}
			return JUInputInstance;
		}
		private static void GetJUInputInstance()
		{
			if (JUInputInstance != null) return;
			JUInputInstance = GameObject.FindFirstObjectByType<JUInputManager>();

			if (JUInputInstance == null)
			{
				JUInputManager NewJUInputManager = new GameObject("JU Input Manager").AddComponent<JUInputManager>();
				JUInputInstance = NewJUInputManager;
				Debug.Log("New JU Input Manager was created because none were found on the scene");
			}
		}

		public enum Axis { MoveHorizontal, MoveVertical, RotateHorizontal, RotateVertical }
		public enum Buttons
		{
			ShotButton, AimingButton, JumpButton, SprintButton, PunchButton,
			RollButton, CrouchButton, ProneButton, ReloadButton,
			PickupButton, EnterVehicleButton, PreviousWeaponButton, NextWeaponButton, OpenInventory
		}

		/// <summary>
		/// Return default axis values.
		/// </summary>
		/// <returns></returns>
		public static float GetAxis(Axis axis)
		{
			GetJUInputInstance();
			switch (axis)
			{
				case Axis.MoveHorizontal:
					return JUInputInstance.MoveHorizontal;

				case Axis.MoveVertical:
					return JUInputInstance.MoveVertical;

				case Axis.RotateHorizontal:
					return JUInputInstance.RotateHorizontal;

				case Axis.RotateVertical:
					return JUInputInstance.RotateVertical;

				default:
					return 0;
			}
		}
		
		/// <summary>
		/// GetButtonDown: UpdateGetButtonDown() 로직에 따라 버튼이 눌린 그 순간에만 true를 반환하고, 그 이후로는 false를 반환 (버튼이 눌려있는 동안 계속해서 true를 반환하는 것이 아니라) 
		/// </summary>
		/// <returns></returns>
		public static bool GetButtonDown(Buttons Button)
		{
			GetJUInputInstance();
			switch (Button)
			{
				case Buttons.ShotButton:
					return JUInputInstance.PressedShootingDown;

				case Buttons.AimingButton:
					return JUInputInstance.PressedAimingDown;

				case Buttons.JumpButton:
					return JUInputInstance.PressedJumpDown;

				case Buttons.SprintButton:
					return JUInputInstance.PressedRunDown;

				case Buttons.PunchButton:
					return JUInputInstance.PressedPunchDown;

				case Buttons.RollButton:
					return JUInputInstance.PressedRollDown;

				case Buttons.CrouchButton:
					return JUInputInstance.PressedCrouchDown;

				case Buttons.ProneButton:
					return JUInputInstance.PressedProneDown;

				case Buttons.ReloadButton:
					return JUInputInstance.PressedReloadDown;

				case Buttons.PickupButton:
					return JUInputInstance.PressedPickupDown;

				case Buttons.EnterVehicleButton:
					return JUInputInstance.PressedInteractDown;

				case Buttons.PreviousWeaponButton:
					return JUInputInstance.PressedPreviousItemDown;

				case Buttons.NextWeaponButton:
					return JUInputInstance.PressedNextItemDown;
				case Buttons.OpenInventory:
					return JUInputInstance.PressedOpenInventoryDown;

				default:
					return false;

			}
		}
		
		/// <summary>
		/// GetButton: UpdateGetButton() 로직에 따라 누르고 있는 동안 true를 반환하는 메서드입니다. 즉, 버튼이 눌리는 동안 계속해서 true를 반환하고, 손을 뗄 때까지 false로 바뀌지 않습니다. 
		/// </summary>
		/// <returns></returns>
		public static bool GetButton(Buttons Button)
		{
			GetJUInputInstance();
			switch (Button)
			{
				case Buttons.ShotButton:
					return JUInputInstance.PressedShooting;

				case Buttons.AimingButton:
					return JUInputInstance.PressedAiming;

				case Buttons.JumpButton:
					return JUInputInstance.PressedJump;

				case Buttons.SprintButton:
					return JUInputInstance.PressedRun;

				case Buttons.PunchButton:
					return JUInputInstance.PressedPunch;

				case Buttons.RollButton:
					return JUInputInstance.PressedRoll;

				case Buttons.CrouchButton:
					return JUInputInstance.PressedCrouch;

				case Buttons.ProneButton:
					return JUInputInstance.PressedProne;

				case Buttons.ReloadButton:
					return JUInputInstance.PressedReload;

				case Buttons.PickupButton:
					return JUInputInstance.PressedPickup;

				case Buttons.EnterVehicleButton:
					return JUInputInstance.PressedInteract;

				case Buttons.PreviousWeaponButton:
					return JUInputInstance.PressedPreviousItem;

				case Buttons.NextWeaponButton:
					return JUInputInstance.PressedNextItem;


				default:
					return false;

			}
		}
		
		/// <summary>
		/// GetButtonUp: 버튼이 눌린 상태에서 손을 뗀 순간 true를 반환하고, 그 외의 경우는 false를 반환
		/// </summary>
		/// <returns></returns>
		public static bool GetButtonUp(Buttons Button)
		{
			GetJUInputInstance();
			switch (Button)
			{
				case Buttons.ShotButton:
					return JUInputInstance.PressedShootingUp;

				case Buttons.AimingButton:
					return JUInputInstance.PressedAimingUp;

				case Buttons.JumpButton:
					return JUInputInstance.PressedJumpUp;

				case Buttons.SprintButton:
					return JUInputInstance.PressedRunUp;

				case Buttons.PunchButton:
					return JUInputInstance.PressedPunchUp;

				case Buttons.RollButton:
					return JUInputInstance.PressedRollUp;

				case Buttons.CrouchButton:
					return JUInputInstance.PressedCrouchUp;

				case Buttons.ProneButton:
					return JUInputInstance.PressedProneUp;

				case Buttons.ReloadButton:
					return JUInputInstance.PressedReloadUp;

				case Buttons.PickupButton:
					return JUInputInstance.PressedPickupUp;

				case Buttons.EnterVehicleButton:
					return JUInputInstance.PressedInteractUp;

				case Buttons.PreviousWeaponButton:
					return JUInputInstance.PressedPreviousItemUp;

				case Buttons.NextWeaponButton:
					return JUInputInstance.PressedNextItemUp;


				default:
					return false;

			}
		}

		/// <summary>
		/// Returns the value of the custom buttons when pressed
		/// </summary>
		/// <param name="CustomButtonName"> The name of the custom button, it must be set to the same name in the JU Input Manager </param>
		/// <returns></returns>
		public static bool GetCustomButton(string CustomButtonName)
		{
			bool value = false;
			for (int i = 0; i < JUInputInstance.CustomButton.Length; i++)
			{
				if (JUInputInstance.CustomButton[i].Name == CustomButtonName)
				{
					value = JUInputInstance.CustomButton[i].Pressed();
				}
				if (JUInputInstance.CustomButton[i].Name != CustomButtonName && i == JUInputInstance.CustomButton.Length)
				{
					Debug.Log("Could not find an input with this name");
					value = false;
				}
			}

			return value;
		}
		/// <summary>
		/// Returns the value of the custom buttons when pressed down
		/// </summary>
		/// <param name="CustomButtonName"> The name of the custom button, it must be set to the same name in the JU Input Manager </param>
		/// <returns></returns>
		public static bool GetCustomButtonDown(string CustomButtonName)
		{
			bool value = false;
			for (int i = 0; i < JUInputInstance.CustomButton.Length; i++)
			{
				if (JUInputInstance.CustomButton[i].Name == CustomButtonName)
				{
					value = JUInputInstance.CustomButton[i].PressedDown();
				}
				if (JUInputInstance.CustomButton[i].Name != CustomButtonName && i == JUInputInstance.CustomButton.Length)
				{
					Debug.Log("Could not find an input with this name");
					value = false;
				}
			}

			return value;
		}

		/// <summary>
		/// Returns the value of the custom buttons when pressed up
		/// </summary>
		/// <param name="CustomButtonName"> The name of the custom button, it must be set to the same name in the JU Input Manager </param>
		/// <returns></returns>
		public static bool GetCustomButtonUp(string CustomButtonName)
		{
			bool value = false;
			for (int i = 0; i < JUInputInstance.CustomButton.Length; i++)
			{
				if (JUInputInstance.CustomButton[i].Name == CustomButtonName)
				{
					value = JUInputInstance.CustomButton[i].PressedUp();
				}
				if (JUInputInstance.CustomButton[i].Name != CustomButtonName && i == JUInputInstance.CustomButton.Length)
				{
					Debug.Log("Could not find an input with this name");
					value = false;
				}
			}

			return value;
		}


		public static Vector2 GetMousePosition()
		{
			if (Instance() == null) return Vector2.zero;
			if (Instance().InputActions == null) return Vector2.zero;
			return Instance().InputActions.Player.MousePosition.ReadValue<Vector2>();
		}


		// 쓰인적 없음 
		public static bool GetKeyDown(UnityEngine.InputSystem.Controls.KeyControl Key)
		{
			return Key.isPressed;
		}


		// 이 아래는 스크린 터치 / 조이스틱 관련 




		/// <summary>
		/// Returns the value of the custom touch buttons when pressed
		/// </summary>
		/// <param name="CustomButtonName"> The name of the custom touch button, it must be set to the same name in the JU Input Manager </param>
		/// <returns></returns>
		public static bool GetCustomTouchButton(string CustomButtonName)
		{
			bool value = false;
			for (int i = 0; i < JUInputInstance.CustomTouchButton.Length; i++)
			{
				if (JUInputInstance.CustomTouchButton[i].Name == CustomButtonName)
				{
					value = JUInputInstance.CustomTouchButton[i].Pressed();
				}
				if (JUInputInstance.CustomTouchButton[i].Name != CustomButtonName && i == JUInputInstance.CustomTouchButton.Length)
				{
					Debug.Log("Could not find an input with this name");
					value = false;
				}
			}

			return value;
		}

		/// <summary> 
		/// Returns the value of the custom touch buttons when pressed down
		/// </summary>
		/// <param name="CustomButtonName"> The name of the custom touch button, it must be set to the same name in the JU Input Manager </param>
		/// <returns></returns>
		public static bool GetCustomTouchButtonDown(string CustomButtonName)
		{
			bool value = false;
			for (int i = 0; i < JUInputInstance.CustomTouchButton.Length; i++)
			{
				if (JUInputInstance.CustomTouchButton[i].Name == CustomButtonName)
				{
					value = JUInputInstance.CustomTouchButton[i].PressedDown();
				}
				if (JUInputInstance.CustomTouchButton[i].Name != CustomButtonName && i == JUInputInstance.CustomTouchButton.Length)
				{
					Debug.Log("Could not find an input with this name");
					value = false;
				}
			}

			return value;
		}

		/// <summary>
		/// Returns the value of the custom touch buttons when pressed up
		/// </summary>
		/// <param name="CustomButtonName"> The name of the custom touch button, it must be set to the same name in the JU Input Manager </param>
		/// <returns></returns>
		public static bool GetCustomTouchButtonUp(string CustomButtonName)
		{
			bool value = false;
			for (int i = 0; i < JUInputInstance.CustomTouchButton.Length; i++)
			{
				if (JUInputInstance.CustomTouchButton[i].Name == CustomButtonName)
				{
					value = JUInputInstance.CustomTouchButton[i].PressedUp();
				}
				if (JUInputInstance.CustomTouchButton[i].Name != CustomButtonName && i == JUInputInstance.CustomTouchButton.Length)
				{
					Debug.Log("Could not find an input with this name");
					value = false;
				}
			}

			return value;
		}

		public static int GetTouchsLengh()
		{
			int touches = -1;
			if (UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.enabled == false)
			{
				UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
				UnityEngine.InputSystem.EnhancedTouch.TouchSimulation.Enable();
				Debug.Log("Started Touch Simulation");
				touches = Touchscreen.current.touches.Count;
			}
			else
			{
				touches = Touchscreen.current.touches.Count;
			}
			return touches;
		}
		public static UnityEngine.InputSystem.Controls.TouchControl[] GetTouches()
		{
			if (UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.enabled == false)
			{
				UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
				UnityEngine.InputSystem.EnhancedTouch.TouchSimulation.Enable();
				Debug.Log("Started Touch Simulation");
			}
			return Touchscreen.current.touches.ToArray();
		}

		/// <summary>
		/// Returns the value of the custom Touchfield - 스크린 터치 
		/// </summary>
		/// <param name="CustomTouchfield"> The name of the custom Touchfield, it must be set to the same name in the JU Input Manager </param>
		/// <returns></returns>
		public static Vector2 GetCustomTouchfieldAxis(string CustomTouchfield)
		{
			Vector2 value = new Vector2(0, 0);
			for (int i = 0; i < JUInputInstance.CustomTouchfield.Length; i++)
			{
				if (JUInputInstance.CustomTouchfield[i].Name == CustomTouchfield)
				{
					value = JUInputInstance.CustomTouchfield[i].TouchDistance();
				}
				if (JUInputInstance.CustomTouchfield[i].Name != CustomTouchfield && i == JUInputInstance.CustomTouchfield.Length)
				{
					Debug.Log("Could not find an input with this name");
				}
			}

			return value;
		}

		/// <summary>
		/// Returns the value of the custom Joystick - 조이스틱 
		/// </summary>
		/// <param name="CustomJoystickName"> The name of the custom Joystick, it must be set to the same name in the JU Input Manager </param>
		/// <returns></returns>
		public static Vector2 GetCustomVirtualJoystickAxis(string CustomJoystickName)
		{
			Vector2 value = new Vector2(0, 0);
			for (int i = 0; i < JUInputInstance.CustomJoystickVirtual.Length; i++)
			{
				if (JUInputInstance.CustomJoystickVirtual[i].Name == CustomJoystickName)
				{
					value = JUInputInstance.CustomJoystickVirtual[i].JoystickInput();
				}
				if (JUInputInstance.CustomJoystickVirtual[i].Name != CustomJoystickName && i == JUInputInstance.CustomJoystickVirtual.Length)
				{
					Debug.Log("Could not find an input with this name");
				}
			}

			return value;
		}

		/// <summary>
		/// Rewrite the value of a default JU input axis - 모바일에서 쓰임 
		/// </summary>
		/// <param name="axis"> The axis thar will be rewritten </param>
		/// <param name="AxisValue">value that will rewrite</param>
		public static void RewriteInputAxis(Axis axis, float AxisValue)
		{
			GetJUInputInstance();
			switch (axis)
			{
				case Axis.MoveHorizontal:
					JUInputInstance.MoveHorizontal = AxisValue;
					break;

				case Axis.MoveVertical:
					JUInputInstance.MoveVertical = AxisValue;
					break;

				case Axis.RotateHorizontal:
					JUInputInstance.RotateHorizontal = AxisValue;
					break;

				case Axis.RotateVertical:
					JUInputInstance.RotateVertical = AxisValue;
					break;

				default:
					Debug.LogWarning("No axis is being rewritten");
					break;

			}
		}

		/// <summary>
		/// Rewrite the value of a default JU input button - 모바일에서 쓰임 
		/// </summary>
		/// <param name="button"> The button thar will be rewritten </param>
		/// <param name="ButtonValue">value that will rewrite</param>
		public static void RewriteInputButtonPressed(Buttons button, bool ButtonValue)
		{
			GetJUInputInstance();
			switch (button)
			{
				case Buttons.ShotButton:
					JUInputInstance.PressedShooting = ButtonValue;
					break;

				case Buttons.AimingButton:
					JUInputInstance.PressedAiming = ButtonValue;
					break;

				case Buttons.JumpButton:
					JUInputInstance.PressedJump = ButtonValue;
					break;

				case Buttons.SprintButton:
					JUInputInstance.PressedRun = ButtonValue;
					break;

				case Buttons.RollButton:
					JUInputInstance.PressedRoll = ButtonValue;
					break;

				case Buttons.CrouchButton:
					JUInputInstance.PressedCrouch = ButtonValue;
					break;

				case Buttons.ReloadButton:
					JUInputInstance.PressedReload = ButtonValue;
					break;

				case Buttons.PickupButton:
					JUInputInstance.PressedPickup = ButtonValue;
					break;

				case Buttons.EnterVehicleButton:
					JUInputInstance.PressedInteract = ButtonValue;
					break;

				case Buttons.PreviousWeaponButton:
					JUInputInstance.PressedPreviousItem = ButtonValue;
					break;

				case Buttons.NextWeaponButton:
					JUInputInstance.PressedNextItem = ButtonValue;
					break;


				default:
					Debug.LogWarning("No button is being rewritten");
					break;
			}
		}
		/// <summary>
		/// Rewrite the value of a default JU input button - 모바일에서 쓰임 
		/// </summary>
		/// <param name="button"> The button thar will be rewritten </param>
		/// <param name="ButtonValue">value that will rewrite</param>
		public static void RewriteInputButtonPressedDown(Buttons button, bool ButtonValue)
		{
			GetJUInputInstance();
			switch (button)
			{
				case Buttons.ShotButton:
					JUInputInstance.PressedShootingDown = ButtonValue;
					break;

				case Buttons.AimingButton:
					JUInputInstance.PressedAimingDown = ButtonValue;
					break;

				case Buttons.JumpButton:
					JUInputInstance.PressedJumpDown = ButtonValue;
					break;

				case Buttons.SprintButton:
					JUInputInstance.PressedRunDown = ButtonValue;
					break;

				case Buttons.RollButton:
					JUInputInstance.PressedRollDown = ButtonValue;
					break;

				case Buttons.CrouchButton:
					JUInputInstance.PressedCrouchDown = ButtonValue;
					break;

				case Buttons.ReloadButton:
					JUInputInstance.PressedReloadDown = ButtonValue;
					break;

				case Buttons.PickupButton:
					JUInputInstance.PressedPickupDown = ButtonValue;
					break;

				case Buttons.EnterVehicleButton:
					JUInputInstance.PressedInteractDown = ButtonValue;
					break;

				case Buttons.PreviousWeaponButton:
					JUInputInstance.PressedPreviousItemDown = ButtonValue;
					break;

				case Buttons.NextWeaponButton:
					JUInputInstance.PressedNextItemDown = ButtonValue;
					break;


				default:
					Debug.LogWarning("No button down is being rewritten");
					break;
			}
		}
		/// <summary>
		/// Rewrite the value of a default JU input button - 모바일에서 쓰임 
		/// </summary>
		/// <param name="button"> The button thar will be rewritten </param>
		/// <param name="ButtonValue">value that will rewrite</param>
		public static void RewriteInputButtonPressedUp(Buttons button, bool ButtonValue)
		{
			GetJUInputInstance();
			switch (button)
			{
				case Buttons.ShotButton:
					JUInputInstance.PressedShootingUp = ButtonValue;
					break;

				case Buttons.AimingButton:
					JUInputInstance.PressedAimingUp = ButtonValue;
					break;

				case Buttons.JumpButton:
					JUInputInstance.PressedJumpUp = ButtonValue;
					break;

				case Buttons.SprintButton:
					JUInputInstance.PressedRunUp = ButtonValue;
					break;

				case Buttons.RollButton:
					JUInputInstance.PressedRollUp = ButtonValue;
					break;

				case Buttons.CrouchButton:
					JUInputInstance.PressedCrouchUp = ButtonValue;
					break;

				case Buttons.ReloadButton:
					JUInputInstance.PressedReloadUp = ButtonValue;
					break;

				case Buttons.PickupButton:
					JUInputInstance.PressedPickupUp = ButtonValue;
					break;

				case Buttons.EnterVehicleButton:
					JUInputInstance.PressedInteractUp = ButtonValue;
					break;

				case Buttons.PreviousWeaponButton:
					JUInputInstance.PressedPreviousItemUp = ButtonValue;
					break;

				case Buttons.NextWeaponButton:
					JUInputInstance.PressedNextItemUp = ButtonValue;
					break;


				default:
					Debug.LogWarning("No button up is being rewritten");
					break;
			}
		}

	}
}
