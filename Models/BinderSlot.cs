namespace PokemonBinder.Models
{
    public class BinderSlot
    {
        public int Id { get; set; }
        public int BinderPageId { get; set; }
        public int SlotPosition { get; set; }
        public int? CardId { get; set; }
        public bool IsPlaceholder { get; set; }
    }
}