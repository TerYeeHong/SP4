public interface IMouseInput
{
    public void UpdateMouseInput();

    public void ReadMouseButtonCommand(MouseButtonCommand mouseButtonCommand);
    public void ReadMouseAxisCommand(MouseAxisCommand mouseAxisCommand);
}
