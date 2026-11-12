namespace HnSF.ui.menus
{
    public class CharacterSelectScreenSelectableCustom : CharacterSelectScreenSelectable
    {
        public override void Submit(int playerIndex)
        {
            base.Submit(playerIndex);
            //screenCharacterSelect.TransitionToCustomCharacterSelect(playerIndex);
        }
    }
}