using Content.Client.UserInterface.Controls;
using Content.Client.VendingMachines.UI;
using Content.Shared.VendingMachines;
using Robust.Client.UserInterface;
using Robust.Shared.Input;
using System.Linq;

namespace Content.Client.VendingMachines
{
    public sealed class VendingMachineBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private VendingMachineMenu? _menu;

        [ViewVariables]
        private List<VendingMachineInventoryEntry> _cachedInventory = new();

        public VendingMachineBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _menu = this.CreateWindowCenteredLeft<VendingMachineMenu>();
            _menu.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
            _menu.OnItemSelected += OnItemSelected;
            Refresh();
            var component = EntMan.GetComponent<VendingMachineComponent>(Owner); //ADT-Economy
            _cachedInventory = vendingMachineSys.GetAllInventory(Owner, component); //ADT-Economy
            _menu.OnClose += Close; //ADT-Economy
            _menu.OnWithdraw += SendMessage; //ADT-Economy

            _menu.Populate(Owner, _cachedInventory, out _cachedFilteredIndex, component.PriceMultiplier, component.Credits); //ADT-Economy
        }

        public void Refresh()
        {
            var system = EntMan.System<VendingMachineSystem>();
            _cachedInventory = system.GetAllInventory(Owner);

            _menu?.Populate(_cachedInventory);
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            if (state is not VendingMachineInterfaceState newState)
                return;

            _cachedInventory = newState.Inventory;

            _menu?.Populate(Owner, _cachedInventory, newState.PriceMultiplier, newState.Credits); //ADT-Economy-Tweak
        }

        private void OnItemSelected(GUIBoundKeyEventArgs args, ListData data)
        {
            if (args.Function != EngineKeyFunctions.UIClick)
                return;

            if (data is not VendorItemsListData { ItemIndex: var itemIndex })
                return;

            _menu?.Populate(Owner, _cachedInventory, out _cachedFilteredIndex, newState.PriceMultiplier, newState.Credits, _menu.SearchBar.Text); //ADT-Economy
        }
        {
            if (_cachedInventory.Count == 0)
                return;

            var selectedItem = _cachedInventory.ElementAtOrDefault(itemIndex);

            if (selectedItem == null)
                return;

            SendMessage(new VendingMachineEjectMessage(selectedItem.Type, selectedItem.ID));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;

            if (_menu == null)
                return;

            _menu.OnItemSelected -= OnItemSelected;
            _menu.OnClose -= Close;
            _menu.Dispose();
        }

        private void OnSearchChanged(string? filter)
        {
            //ADT-Economy-Start
            var component = EntMan.GetComponent<VendingMachineComponent>(Owner);
            _menu?.Populate(Owner, _cachedInventory, out _cachedFilteredIndex, component.PriceMultiplier, component.Credits, filter);
            //ADT-Economy-End
        }
    }
}
