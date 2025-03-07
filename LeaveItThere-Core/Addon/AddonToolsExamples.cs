using LeaveItThere.Common;
using LeaveItThere.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveItThere.Addon
{
    internal class AddonToolsExamples
    {
        public void OnFakeItemInitialized(FakeItem fakeItem)
        {
            // to target a Grizzly medkit, return when the item's template id is not that of a Grizzly
            if (fakeItem.TemplateId != "590c657e86f77412b013051d") return;

            // if it is a Grizzly, add a new interaction to it
            fakeItem.Interactions.Add(new MySimpleCustomInteraction());
        }

        public class MySimpleCustomInteraction : CustomInteraction
        {
            public override string Name => "My Simple Interaction";

            public override void OnInteract()
            {
                NotificationManagerClass.DisplayMessageNotification("My Simple Interction selected!");
            }
        }
    }
}
