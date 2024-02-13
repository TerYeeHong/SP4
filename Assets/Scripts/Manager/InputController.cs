using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class InputController : MonoBehaviour
{
    IKeyInput keyInputControl;
    IMouseInput mouseInputControl;

    private MovementAxisCommand _lastMovementAxisCommand = new MovementAxisCommand(0, 0, 0);
    private MouseAxisCommand _lastMouseAxisCommand = new MouseAxisCommand(0, 0, 0, new Vector2(0, 0));

    //For command and replay system
    string file_path = "/replayfile.txt";
    string line_end = "@";
    Queue<Command> command_file_command_queue = new();

    float _replayButtonToggledTime;
    float _replayTimeNextCommand;
    float _replayTimer = 0;
    REPLAY_STATUS _replayStatusCurrent = REPLAY_STATUS.NONE;
    public enum REPLAY_STATUS
    {
        NONE = 0,
        WRITING_INPUT,
        READING_INPUT,
    }


    IEnumerator PlayCommand()
    {
        GetNextCommandTime();
        _replayButtonToggledTime = Time.time;

        //May have multiple commands recorded at the same time, hence loop
        while (HaveNextCommand())
        {

            //if (_replayTimeNextCommand > _replayTimer)
            //{
            //    yield return new WaitForSeconds(_replayTimeNextCommand - _replayTimer);
            //}
            //_replayTimer = _replayTimeNextCommand;


            //can do next command
            if (_replayTimeNextCommand <= Time.time - _replayButtonToggledTime)
            {
                //Handle the command
                Command command = GetNextCommand();
                switch (command.command_type)
                {
                    case Command.COMMAND_TYPE.MOVE_AXIS:
                        keyInputControl?.ReadMovementAxisCommand((MovementAxisCommand)command);
                        break;
                    case Command.COMMAND_TYPE.MOUSE_AXIS:
                        mouseInputControl?.ReadMouseAxisCommand((MouseAxisCommand)command);
                        break;
                    case Command.COMMAND_TYPE.MOVE_BUTTON:
                        keyInputControl?.ReadMovementButtonCommand((MovementButtonCommand)command);
                        break;
                    case Command.COMMAND_TYPE.MOUSE_BUTTON:
                        mouseInputControl?.ReadMouseButtonCommand((MouseButtonCommand)command);
                        break;
                    case Command.COMMAND_TYPE.ABILITY:
                        keyInputControl?.ReadAbilityCommand((AbilityCommand)command);
                        break;
                }

                if (HaveNextCommand())
                GetNextCommandTime();


            }

            yield return null;


            ////Handle the command
            //Command command = GetNextCommand();
            //switch (command.command_type)
            //{
            //    case Command.COMMAND_TYPE.MOVE_AXIS:
            //        keyInputControl?.ReadMovementAxisCommand((MovementAxisCommand)command);
            //        break;
            //    case Command.COMMAND_TYPE.MOUSE_AXIS:
            //        mouseInputControl?.ReadMouseAxisCommand((MouseAxisCommand)command);
            //        break;
            //    case Command.COMMAND_TYPE.MOVE_BUTTON:
            //        keyInputControl?.ReadMovementButtonCommand((MovementButtonCommand)command);
            //        break;
            //    case Command.COMMAND_TYPE.MOUSE_BUTTON:
            //        mouseInputControl?.ReadMouseButtonCommand((MouseButtonCommand)command);
            //        break;
            //    case Command.COMMAND_TYPE.ABILITY:
            //        keyInputControl?.ReadAbilityCommand((AbilityCommand)command);
            //        break;
            //}
        }

        _replayStatusCurrent = REPLAY_STATUS.NONE;
        GameEvents.m_instance.updateReplayStatus.Invoke(REPLAY_STATUS.NONE, 0);
    }

    // Update is called once per frame
    //void Update()
    //{
    //    switch (_replayStatusCurrent)
    //    {
    //        case REPLAY_STATUS.WRITING_INPUT:
    //            GameEvents.m_instance.updateReplayStatus.Invoke(_replayStatusCurrent, Time.time - _replayButtonToggledTime);
    //            break;
    //        case REPLAY_STATUS.READING_INPUT:
    //            GameEvents.m_instance.updateReplayStatus.Invoke(_replayStatusCurrent, Time.time - _replayButtonToggledTime);
    //            break;
    //    }


    //    //Take player input only if not trying to read commands from a file
    //    if (_replayStatusCurrent != REPLAY_STATUS.READING_INPUT)
    //    {
    //        //Check if want to replay
    //        if (Input.GetKeyDown(KeyCode.T))
    //        {
    //            if (_replayStatusCurrent == REPLAY_STATUS.WRITING_INPUT)
    //                EndRecordReplay();
    //            StartReplay();
    //            return;
    //        }

    //        //Check if want to Record (toggle)
    //        if (Input.GetKeyDown(KeyCode.R)) {
    //            if (_replayStatusCurrent == REPLAY_STATUS.NONE)
    //                StartRecordReplay();
    //            else
    //                EndRecordReplay();
    //            return;
    //        }

    //        //Current mouse and key input controllers
    //        if (keyInputControl != null)
    //        {
    //            //Detect input, to be used
    //            if (TryGetMovementAxisInput(out MovementAxisCommand move) )
    //            //if (TryGetMovementAxisInput(out MovementAxisCommand move) || true)
    //                {
    //                keyInputControl.ReadMovementAxisCommand(move);

    //                //If suppose to save these replays
    //                if (_replayStatusCurrent == REPLAY_STATUS.WRITING_INPUT)
    //                    TextFileManager.WriteString(file_path, move.ToString() + line_end);
    //            }
    //            if (TryGetBasicAbility(out AbilityCommand basic_ability))
    //            {
    //                keyInputControl.ReadAbilityCommand(basic_ability);

    //                //If suppose to save these replays
    //                if (_replayStatusCurrent == REPLAY_STATUS.WRITING_INPUT)
    //                    TextFileManager.WriteString(file_path, basic_ability.ToString() + line_end);
    //            }
    //            if (TryGetMovementButton(out MovementButtonCommand move_button))
    //            {
    //                keyInputControl.ReadMovementButtonCommand(move_button);

    //                //If suppose to save these replays
    //                if (_replayStatusCurrent == REPLAY_STATUS.WRITING_INPUT)
    //                    TextFileManager.WriteString(file_path, move_button.ToString() + line_end);
    //            }
    //        }
    //        if (mouseInputControl != null)
    //        {
    //            //Detect input, to be used
    //            if (TryGetMouseAxisInput(out MouseAxisCommand mouse_axis))
    //            {
    //                mouseInputControl.ReadMouseAxisCommand(mouse_axis);

    //                //If suppose to save these replays
    //                if (_replayStatusCurrent == REPLAY_STATUS.WRITING_INPUT)
    //                    TextFileManager.WriteString(file_path, mouse_axis.ToString() + line_end);
    //            }
    //            if (TryGetMouseButton(out MouseButtonCommand mouse_button))
    //            {
    //                mouseInputControl.ReadMouseButtonCommand(mouse_button);

    //                //If suppose to save these replays
    //                if (_replayStatusCurrent == REPLAY_STATUS.WRITING_INPUT)
    //                    TextFileManager.WriteString(file_path, mouse_button.ToString() + line_end);
    //            }
    //        }
    //    }

    //    //have to update every frame
    //    keyInputControl?.UpdateKeyInput();
    //    mouseInputControl?.UpdateMouseInput();

    //}


    void Update()
    {
        switch (_replayStatusCurrent)
        {
            case REPLAY_STATUS.WRITING_INPUT:
                GameEvents.m_instance.updateReplayStatus.Invoke(_replayStatusCurrent, Time.time - _replayButtonToggledTime);
                break;
            case REPLAY_STATUS.READING_INPUT:
                GameEvents.m_instance.updateReplayStatus.Invoke(_replayStatusCurrent, Time.time - _replayButtonToggledTime);
                break;
        }

        MovementAxisCommand move = null;
        AbilityCommand basic_ability = null;
        MovementButtonCommand move_button = null;
        MouseAxisCommand mouse_axis = null;
        MouseButtonCommand mouse_button = null;

        //Take player input only if not trying to read commands from a file
        if (_replayStatusCurrent != REPLAY_STATUS.READING_INPUT)
        {
            //Check if want to replay
            if (Input.GetKeyDown(KeyCode.T))
            {
                if (_replayStatusCurrent == REPLAY_STATUS.WRITING_INPUT)
                    EndRecordReplay();
                StartReplay();
                return;
            }

            //Check if want to Record (toggle)
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (_replayStatusCurrent == REPLAY_STATUS.NONE)
                    StartRecordReplay();
                else
                    EndRecordReplay();
                return;
            }

            //Detect input, to be used
            if (TryGetMovementAxisInput(out move))
            //if (TryGetMovementAxisInput(out MovementAxisCommand move) || true)
            {
                ////If suppose to save these replays
                //if (_replayStatusCurrent == REPLAY_STATUS.WRITING_INPUT)
                //    TextFileManager.WriteString(file_path, move.ToString() + line_end);
            }
            if (TryGetBasicAbility(out basic_ability))
            {

                ////If suppose to save these replays
                //if (_replayStatusCurrent == REPLAY_STATUS.WRITING_INPUT)
                //    TextFileManager.WriteString(file_path, basic_ability.ToString() + line_end);
            }
            if (TryGetMovementButton(out move_button))
            {

                ////If suppose to save these replays
                //if (_replayStatusCurrent == REPLAY_STATUS.WRITING_INPUT)
                //    TextFileManager.WriteString(file_path, move_button.ToString() + line_end);
            }

            //Detect input, to be used
            if (TryGetMouseAxisInput(out mouse_axis))
            {

                ////If suppose to save these replays
                //if (_replayStatusCurrent == REPLAY_STATUS.WRITING_INPUT)
                //    TextFileManager.WriteString(file_path, mouse_axis.ToString() + line_end);
            }
            if (TryGetMouseButton(out mouse_button))
            {

                ////If suppose to save these replays
                //if (_replayStatusCurrent == REPLAY_STATUS.WRITING_INPUT)
                //    TextFileManager.WriteString(file_path, mouse_button.ToString() + line_end);
            }

        }
        //Else is reading inputs
        else
        {
            if (HaveNextCommand())
                GetNextCommandTime();
            else
            {
                _replayStatusCurrent = REPLAY_STATUS.NONE;
                GameEvents.m_instance.updateReplayStatus.Invoke(REPLAY_STATUS.NONE, 0);
                return;
            }

            //can do next command
            if (_replayTimeNextCommand - 0.05f <= Time.time - _replayButtonToggledTime)
            {
                //Handle the command
                Command command = GetNextCommand();
                switch (command.command_type)
                {
                    case Command.COMMAND_TYPE.MOVE_AXIS:
                        move = ((MovementAxisCommand)command);
                        break;
                    case Command.COMMAND_TYPE.MOUSE_AXIS:
                        mouse_axis = ((MouseAxisCommand)command);
                        break;
                    case Command.COMMAND_TYPE.MOVE_BUTTON:
                        move_button = ((MovementButtonCommand)command);
                        break;
                    case Command.COMMAND_TYPE.MOUSE_BUTTON:
                        mouse_button = ((MouseButtonCommand)command);
                        break;
                    case Command.COMMAND_TYPE.ABILITY:
                        basic_ability = ((AbilityCommand)command);
                        break;
                }

            }
        }

        //Current mouse and key input controllers
        if (keyInputControl != null)
        {
            if (move != null)
            keyInputControl.ReadMovementAxisCommand(move);
            if (basic_ability != null)
            keyInputControl.ReadAbilityCommand(basic_ability);
            if (move_button != null)
            keyInputControl.ReadMovementButtonCommand(move_button);

        }
        if (mouseInputControl != null)
        {
            if (mouse_axis != null)
            mouseInputControl.ReadMouseAxisCommand(mouse_axis);
            if (mouse_button != null)
            mouseInputControl.ReadMouseButtonCommand(mouse_button);

        }


        //have to update every frame
        keyInputControl?.UpdateKeyInput();
        mouseInputControl?.UpdateMouseInput();

    }
    public void StartRecordReplay()
    {
        //TextFileManager.ClearFile(file_path);

        //_replayButtonToggledTime = Time.time;
        //_replayStatusCurrent = REPLAY_STATUS.WRITING_INPUT;
    }
    public void EndRecordReplay()
    {
        //_replayStatusCurrent = REPLAY_STATUS.NONE;
        //GameEvents.m_instance.updateReplayStatus.Invoke(REPLAY_STATUS.NONE, 0);

    }
    public void StartReplay()
    {
        //_replayButtonToggledTime = Time.time;

        ////Store all the commands here
        ////command_file_command_queue = new();
        //command_file_command_queue.Clear();

        //string command_file_text = TextFileManager.ReadFile(file_path);
        //Debug.Log(command_file_text);

        //var commands_string = command_file_text.Split("@");

        ////Adds all the commands into a queue
        //foreach (string command_string in commands_string)
        //{
        //    if (command_string == "")
        //        break;
            
        //    var command = Command.FromString(command_string);
        //    command_file_command_queue.Enqueue(command);
        //}

        //_replayTimer = 0;
        //_replayStatusCurrent = REPLAY_STATUS.READING_INPUT;

        ////StartCoroutine(PlayCommand());
    }
    Command GetNextCommand()
    {
        return command_file_command_queue.Dequeue();
    }
    void GetNextCommandTime()
    {
        _replayTimeNextCommand = command_file_command_queue.Peek().Time;
    }
    bool HaveNextCommand()
    {
        return (command_file_command_queue.Count != 0);
    }



    private void OnEnable()
    {
        GameEvents.m_instance.useNewKeyInput.AddListener(OnUseNewKeyInput);
        GameEvents.m_instance.useNewMouseInput.AddListener(OnUseNewMouseInput);
    }
    private void OnDisable()
    {
        GameEvents.m_instance.useNewKeyInput.RemoveListener(OnUseNewKeyInput);
        GameEvents.m_instance.useNewMouseInput.RemoveListener(OnUseNewMouseInput);
    }

    public void OnUseNewKeyInput(IKeyInput iKeyInput)
    {
        keyInputControl = iKeyInput;
    }
    public void OnUseNewMouseInput(IMouseInput iMouseInput)
    {
        mouseInputControl = iMouseInput;
    }



    public bool TryGetMovementAxisInput(out MovementAxisCommand movementAxisCommand)
    {
        float horizontalAxis = Input.GetAxisRaw("Horizontal");
        float verticalAxis = Input.GetAxisRaw("Vertical");
        //bool hasAxisInputChanged = _lastMovementAxisCommand.HorizontalAxis != horizontalAxis || _lastMovementAxisCommand.VerticalAxis != verticalAxis;
        bool hasAxisInputChanged = 0 != horizontalAxis || 0 != verticalAxis;

        //if (hasAxisInputChanged)
        //    _lastMovementAxisCommand = new MovementAxisCommand(Time.time - _replayButtonToggledTime, horizontalAxis, verticalAxis);
        //movementAxisCommand = _lastMovementAxisCommand;

        movementAxisCommand = null;
        if (hasAxisInputChanged)
            movementAxisCommand = new MovementAxisCommand(Time.time - _replayButtonToggledTime, horizontalAxis, verticalAxis);

        return hasAxisInputChanged;
    }
    public bool TryGetMouseAxisInput(out MouseAxisCommand mouseAxisCommand)
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        Vector2 mouseDelta = Input.mouseScrollDelta;

        //bool hasAxisInputChanged = _lastMouseAxisCommand.MouseX != mouseX || _lastMouseAxisCommand.MouseY != mouseY
        //    || mouseDelta.x != 0 || mouseDelta.y != 0;
        bool hasAxisInputChanged = 0 != mouseX || 0 != mouseY
            || mouseDelta.x != 0 || mouseDelta.y != 0;

        //if (hasAxisInputChanged)
        //    _lastMouseAxisCommand = new MouseAxisCommand(Time.time - _replayButtonToggledTime, mouseX, mouseY, mouseDelta);
        //mouseAxisCommand = _lastMouseAxisCommand;

        mouseAxisCommand = null;
        if (hasAxisInputChanged)
            mouseAxisCommand = new MouseAxisCommand(Time.time - _replayButtonToggledTime, mouseX, mouseY, mouseDelta);


        return hasAxisInputChanged;
    }
    public bool TryGetMouseButton(out MouseButtonCommand mouseButtonCommand)
    {
        mouseButtonCommand = null;
        bool mouseButtonDown = Input.GetMouseButtonDown(0);
        bool mouseButtonHold = Input.GetMouseButton(0);
        bool mouseButtonUp = Input.GetMouseButtonUp(0);

        if (mouseButtonDown || mouseButtonHold || mouseButtonUp)
            mouseButtonCommand = new MouseButtonCommand(Time.time - _replayButtonToggledTime, mouseButtonDown, mouseButtonHold, mouseButtonUp);
        return mouseButtonCommand != null;
    }
    public bool TryGetBasicAbility(out AbilityCommand basicAbilityCommand)
    {
        basicAbilityCommand = null;
        bool fireDown = Input.GetButtonDown("Fire1");
        bool fireHold = Input.GetButton("Fire1");
        bool fireUp = Input.GetButtonUp("Fire1");
        bool aimHold = Input.GetButton("Fire2");

        bool hasInputChanged 
            = fireDown != false 
            || fireHold != false
            || fireUp != false
            || aimHold != false;
        if (hasInputChanged)
            basicAbilityCommand = new AbilityCommand(Time.time - _replayButtonToggledTime, 
                fireDown, fireHold, fireUp, aimHold);

        return hasInputChanged;
    }
    public bool TryGetMovementButton(out MovementButtonCommand movementButtonCommand)
    {
        movementButtonCommand = null;
        bool jumpDown = Input.GetButtonDown("Jump");
        bool crouchDown = Input.GetButtonDown("Crouch");
        bool sprintHold = Input.GetButton("Sprint");
        bool sprintDown = Input.GetButtonDown("Sprint");

        bool hasInputChanged 
            = jumpDown != false 
            || crouchDown != false 
            || sprintHold != false
            || sprintDown != false;
        if (hasInputChanged)
            movementButtonCommand = new MovementButtonCommand(Time.time - _replayButtonToggledTime, 
                jumpDown, crouchDown, sprintHold, sprintDown);

        return hasInputChanged;
    }
}

public abstract class Command
{
    public float Time { get; private set; }
    public Command(float time) => Time = time;
    public COMMAND_TYPE command_type;

    public enum COMMAND_TYPE
    {
        MOVE_AXIS,
        MOUSE_AXIS,

        MOVE_BUTTON,
        MOUSE_BUTTON,

        ABILITY,
    }


    //public override string ToString() => $"{(int)command_type},{Time}";
    public static Command FromString(string inString)
    {
        //$"{(int)command_type},{Time},{Data}"
        //data[0] = command_type in Int
        //data[1] = time in float
        //data[2] = list of value for command in string[] 
        var data = inString.Split(",");
        var values = data[2].Split("!");

        switch ((COMMAND_TYPE)int.Parse(data[0]))
        {
            case COMMAND_TYPE.MOVE_AXIS:
                return new MovementAxisCommand(
                    float.Parse(data[1]), 
                    float.Parse(values[0]), 
                    float.Parse(values[1]));
            case COMMAND_TYPE.MOUSE_AXIS:
                return new MouseAxisCommand(
                    float.Parse(data[1]),
                    float.Parse(values[0]), 
                    float.Parse(values[1]), 
                    new Vector2(float.Parse(values[2]), float.Parse(values[3])));
            case COMMAND_TYPE.MOVE_BUTTON:
                return new MovementButtonCommand(
                    float.Parse(data[1]),
                    bool.Parse(values[0]),
                    bool.Parse(values[1]),
                    bool.Parse(values[2]),
                    bool.Parse(values[3]));
            case COMMAND_TYPE.MOUSE_BUTTON:
                return new MouseButtonCommand(
                    float.Parse(data[1]),
                    bool.Parse(values[0]),
                    bool.Parse(values[1]),
                    bool.Parse(values[2]));
            case COMMAND_TYPE.ABILITY:
                return new AbilityCommand(
                    float.Parse(data[1]),
                    bool.Parse(values[0]),
                    bool.Parse(values[1]),
                    bool.Parse(values[2]),
                    bool.Parse(values[3]));
        }

        Debug.LogWarning("Trying to read Command, it doesnt have a type?");
        return null;


        //return new Command(
        //    inAction: (ActionType)Enum.Parse(typeof(ActionType), data[0]),
        //    inState: (KeyState)Enum.Parse(typeof(KeyState), data[3]),
        //    inTime: float.Parse(data[4]);
    }
}

public class MovementAxisCommand : Command
{
    public float HorizontalAxis { get; private set; }
    public float VerticalAxis { get; private set; }

    public MovementAxisCommand(float time, float horizontalAxis, float verticalAxis) : base(time)
    {
        HorizontalAxis = horizontalAxis;
        VerticalAxis = verticalAxis;

        command_type = COMMAND_TYPE.MOVE_AXIS;
    }

    public override string ToString() => 
        $"{(int)command_type}," +
        $"{Time}," +
        $"{$"{HorizontalAxis}!{VerticalAxis}"}";
}
public class MouseAxisCommand : Command
{
    public float MouseX { get; private set; }
    public float MouseY { get; private set; }
    public Vector2 MouseDelta { get; private set; }

    public MouseAxisCommand(float time, float mouseX, float mouseY, Vector2 mouseDelta) : base(time)
    {
        MouseX = mouseX;
        MouseY = mouseY;
        MouseDelta = mouseDelta;

        command_type = COMMAND_TYPE.MOUSE_AXIS;
    }

    public override string ToString() =>
        $"{(int)command_type}," +
        $"{Time}," +
        $"{$"{MouseX}!{MouseY}!{MouseDelta.x}!{MouseDelta.y}"}";
}
public class MouseButtonCommand : Command
{
    public bool MouseButtonDown { get; private set; }
    public bool MouseButtonHold { get; private set; }
    public bool MouseButtonUp { get; private set; }

    public MouseButtonCommand(float time, bool mouseButtonDown, bool mouseButtonHold, bool mouseButtonUp) : base(time)
    {
        MouseButtonDown = mouseButtonDown;
        MouseButtonHold = mouseButtonHold;
        MouseButtonUp = mouseButtonUp;

        command_type = COMMAND_TYPE.MOUSE_BUTTON;
    }

    public override string ToString() =>
        $"{(int)command_type}," +
        $"{Time}," +
        $"{$"{MouseButtonDown}!{MouseButtonHold}!{MouseButtonUp}"}";
}
public class AbilityCommand : Command
{
    public bool FireDown { get; private set; }
    public bool FireHold { get; private set; }
    public bool FireUp { get; private set; }
    public bool AimHold { get; private set; }

    public AbilityCommand(float time, bool fireDown, bool fireHold, bool fireUp, bool aimHold) : base(time)
    {
        FireDown = fireDown;
        FireHold = fireHold;
        FireUp = fireUp;
        AimHold = aimHold;

        command_type = COMMAND_TYPE.ABILITY;
    }

    public override string ToString() =>
        $"{(int)command_type}," +
        $"{Time}," +
        $"{$"{FireDown}!{FireHold}!{FireUp}!{AimHold}"}";
}
public class MovementButtonCommand : Command
{
    public bool JumpDown { get; private set; }
    public bool CrouchDown { get; private set; }
    public bool SprintHold { get; private set; }
    public bool SprintDown { get; private set; }

    public MovementButtonCommand(float time, bool jumpDown, bool crouchDown, bool sprintHold, bool sprintDown) : base(time)
    {
        JumpDown = jumpDown;
        CrouchDown = crouchDown;
        SprintHold = sprintHold;
        SprintDown = sprintDown;

        command_type = COMMAND_TYPE.MOVE_BUTTON;
    }

    public override string ToString() =>
        $"{(int)command_type}," +
        $"{Time}," +
        $"{$"{JumpDown}!{CrouchDown}!{SprintHold}!{SprintDown}"}";
}