namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a specification attribute type
    /// </summary>
    public enum SpecificationAttributeType
    {
        /// <summary>
        /// Option
        /// </summary>
        Option = 0,
        /// <summary>
        /// Custom text
        /// </summary>
        CustomText = 10,
        /// <summary>
        /// Custom HTML text
        /// </summary>
        CustomHtmlText = 20,
        /// <summary>
        /// Hyperlink
        /// </summary>
        Hyperlink = 30,
        /// <summary>
        /// Custom Color
        /// </summary>
        CustomColor = 40,
        /// <summary>
        /// Custom Image
        /// </summary>
        CustomImage = 50,
    }
}