using System;
using System.Collections.Generic;

namespace LeaveItThere.Common
{
    public class CustomInteraction
    {
        public Action Action;
        public Func<string> GetName;
        public Func<bool> GetDisabled;

        /// <summary>
        /// TargetName only works if it is on the very first (top) interaction.
        /// </summary>
        public string TargetName = null;

        /// <summary>
        /// Custom Interaction Action to be added to an interactable prompt<br/>
        /// </summary>
        /// <remarks>
        /// name can be a <see cref="string"/> or a <see cref="Func{}"/> that returns <see cref="string"/><br/>
        /// disabled can be a <see cref="bool"/> or a <see cref="Func{}"/> that returns <see cref="bool"/><br/>
        /// </remarks>
        public CustomInteraction(string name, bool disabled, Action action) // simple static name and disabled
        {
            GetName = () => name;
            GetDisabled = () => disabled;
            Action = action;
        }

        public CustomInteraction(Func<string> getName, Func<bool> getDisabled, Action action) // dynamic simple and static
        {
            GetName = getName;
            GetDisabled = getDisabled;
            Action = action;
        }

        public CustomInteraction(string name, Func<bool> getDisabled, Action action) // simple name, dynamic disabled
        {
            GetName = () => name;
            GetDisabled = getDisabled;
            Action = action;
        }

        public CustomInteraction(Func<string> getName, bool disabled, Action action) // dynamic name, simple disabled
        {
            GetName = getName;
            GetDisabled = () => disabled;
            Action = action;
        }

        public ActionsTypesClass GetActionsTypesClass()
        {
            ActionsTypesClass typesClass = new()
            {
                Action = Action,
                Name = GetName(),
                Disabled = GetDisabled(),
            };

            typesClass.TargetName = TargetName;

            return typesClass;
        }

        public static List<ActionsTypesClass> GetActionsTypesClassList(List<CustomInteraction> CustomInteractionActionList)
        {
            List<ActionsTypesClass> actionsTypesClassList = [];

            foreach (CustomInteraction CustomInteractionAction in CustomInteractionActionList)
            {
                actionsTypesClassList.Add(CustomInteractionAction.GetActionsTypesClass());
            }

            return actionsTypesClassList;
        }
    }

}
