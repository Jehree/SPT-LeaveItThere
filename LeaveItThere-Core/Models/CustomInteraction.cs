using LeaveItThere.Components;
using LeaveItThere.Helpers;
using System.Collections.Generic;

namespace LeaveItThere.Common;

public class CustomInteractionContainer(FakeItem fakeItem)
{
    public List<CustomInteraction> CustomInteractions { get; set; } = [];

    public string TargetName => fakeItem.LootItem.Name.Localized();

    private ActionsReturnClass _actionsReturnClass = null;

    public ActionsReturnClass ActionsReturnClass
    {
        get
        {
            _actionsReturnClass ??= new()
            {
                Actions = []
            };

            _actionsReturnClass.Actions.Clear();

            foreach (CustomInteraction interaction in CustomInteractions)
            {
                _actionsReturnClass.Actions.Add(interaction.ActionsTypesClass);
            }

            if (_actionsReturnClass.Actions.IsNullOrEmpty())
            {
                _actionsReturnClass.Actions[0].TargetName = TargetName;
            }

            return _actionsReturnClass;
        }
    }
}

public abstract class CustomInteraction
{
    /// <summary>
    /// If returns false, interaction prompt will be greyed out and not selectable.
    /// </summary>
    public virtual bool Enabled { get => true; }

    /// <summary>
    /// If returns true, the interaction will auto refresh after Action() is called. This will update any names or states of the interaction, but will also reset the selected action to the first one.
    /// </summary>
    public virtual bool AutoPromptRefresh { get => false; }

    /// <summary>
    /// Name of interaction item in prompt.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Interaction callback. Will be called when an interaction is interacted with.
    /// </summary>
    public abstract void OnInteract();

    private ActionsTypesClass _actionsTypesClass = null;
    public ActionsTypesClass ActionsTypesClass
    {
        get
        {
            _actionsTypesClass ??= new();

            _actionsTypesClass.Action = AutoPromptRefresh
                ? OnInteract + InteractionHelper.RefreshPromptAction
                : OnInteract;
            _actionsTypesClass.Name = Name;
            _actionsTypesClass.Disabled = !Enabled;

            return _actionsTypesClass;
        }
    }

    public class DisabledInteraction(string name) : CustomInteraction
    {
        public override string Name => name;
        public override bool Enabled => false;
        public override void OnInteract() { }
    }
}