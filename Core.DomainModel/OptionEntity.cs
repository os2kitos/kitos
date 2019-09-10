namespace Core.DomainModel
{
    /// <summary>
    /// Defines interface for objects that have a 1:M option choice in another object.
    /// In practice this will be dropdown menus.
    /// </summary>
    /// <typeparam name="TReference">Type of the object that this option relates to.</typeparam>
    /// <remarks>
    /// These types of OptionEntities can only be used by one <see cref="References"/>.
    /// </remarks>
    public abstract class OptionEntity<TReference> : Entity
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <remarks>
        /// This is known as Title in OIO.
        /// </remarks>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsLocallyAvailable { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is obligatory.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsObligatory { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is a suggestion.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is suggestion; otherwise, <c>false</c>.
        /// </value>
        public bool IsSuggestion { get; set; }
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public int Priority { get; set; }
    }
}
