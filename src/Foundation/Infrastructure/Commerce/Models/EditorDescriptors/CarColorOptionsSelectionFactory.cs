namespace Foundation.Infrastructure.Commerce.Models.EditorDescriptors
{
    public class CarColorOptionSelectionFactory : ISelectionFactory
    {
        public virtual IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            return new ISelectItem[]
            {
                new SelectItem { Text = "Red", Value = "Red" },
                new SelectItem { Text = "White", Value = "White" },
                new SelectItem { Text = "Black", Value = "Black" }
            };
        }
    }
}