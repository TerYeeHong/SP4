public interface IKeyInput
{
    public void UpdateKeyInput();


    public void ReadMovementAxisCommand(MovementAxisCommand movementAxisCommand);
    public void ReadAbilityCommand(AbilityCommand basicAbilityCommand);
    public void ReadMovementButtonCommand(MovementButtonCommand movementButtonCommand);
}
