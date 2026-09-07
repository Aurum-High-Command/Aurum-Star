namespace Content.Shared._Goobstation.Clothing;

[ByRefEvent]
public record struct CheckClothingSlotHiddenEvent(string Slot, bool Visible = true);
