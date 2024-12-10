using System;
using System.Collections.Generic;

namespace InteractableInteractionsAPI.Common
{
    public class CustomInteractionAction
    {
        public Action Action;
        public Func<string> GetName { get; private set; }
        public Func<bool> GetDisabled { get; private set; }

        /// <summary>
        /// Custom Interaction Action to be added to an interactable prompt<br/>
        /// </summary>
        /// <remarks>
        /// name can be a <see cref="string"/> or a <see cref="Func{}"/> that returns <see cref="string"/><br/>
        /// disabled can be a <see cref="bool"/> or a <see cref="Func{}"/> that returns <see cref="bool"/><br/>
        /// </remarks>
        public CustomInteractionAction(string name, bool disabled, Action action) // simple static name and disabled
        {
            GetName = () => name;
            GetDisabled = () => disabled;
            Action = action;
        }

        public CustomInteractionAction(Func<string> getName, Func<bool> getDisabled, Action action) // dynamic simple and static
        {
            GetName = getName;
            GetDisabled = getDisabled;
            Action = action;
        }

        public CustomInteractionAction(string name, Func<bool> getDisabled, Action action) // simple name, dynamic disabled
        {
            GetName = () => name;
            GetDisabled = getDisabled;
            Action = action;
        }

        public CustomInteractionAction(Func<string> getName, bool disabled, Action action) // dynamic name, simple disabled
        {
            GetName = getName;
            GetDisabled = () => disabled;
            Action = action;
        }

        public ActionsTypesClass GetActionsTypesClass()
        {
            return new ActionsTypesClass
            {
                Action = Action,
                Name = GetName(),
                Disabled = GetDisabled(),
            };
        }

        public static List<ActionsTypesClass> GetActionsTypesClassList(List<CustomInteractionAction> CustomInteractionActionList)
        {
            List<ActionsTypesClass> actionsTypesClassList = new List<ActionsTypesClass>();

            foreach (CustomInteractionAction CustomInteractionAction in CustomInteractionActionList)
            {
                actionsTypesClassList.Add(CustomInteractionAction.GetActionsTypesClass());
            }

            return actionsTypesClassList;
        }
    }

}
