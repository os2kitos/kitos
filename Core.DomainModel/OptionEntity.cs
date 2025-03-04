using System;
using Core.Abstractions.Types;

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
    public abstract class OptionEntity<TReference> : Entity, IHasName, IHasUuid
    {
        public static int MaxNameLength = 150;

        protected OptionEntity()
        {
            Uuid = Guid.NewGuid();
        }

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
        /// NOTE: This value is confusing since the db value does not reflect reality.
        /// It is set by the controllers and is set based on <see cref="IsObligatory"/> OR <see cref="LocalOptionEntity{OptionType}.IsActive"/>
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
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public int Priority { get; set; }
        public Guid Uuid { get; set; }

        public void UpdateLocalOptionValues<TLocal>(LocalOptionEntity<TLocal>? localOption)
        {
            if (localOption == null)
            {
                ResetLocalOptionAvailability();
                return;
            }
            UpdateIsLocallyAvailable(localOption.IsActive);
            UpdateDescription(localOption.Description);
        }

        public void ResetLocalOptionAvailability()
        {
            IsLocallyAvailable = false;
        }

        private void UpdateIsLocallyAvailable(bool isActive)
        {
            if (IsObligatory && !isActive) return;
            if (IsEnabled) IsLocallyAvailable = isActive;
        }

        private void UpdateDescription(string description)
        {
           if (!string.IsNullOrEmpty(description)) Description = description;
        }

        public void UpdateDescription(Maybe<string> description)
        {
            Description = description.HasValue ? description.Value : null;
        }

        public void UpdateName(Maybe<string> name)
        {
            Name = name.HasValue ? name.Value : null;
        }

        public void UpdateIsObligatory(Maybe<bool> isObligatory)
        {
            IsObligatory = isObligatory.HasValue && isObligatory.Value;
        }

        public void UpdateIsEnabled(Maybe<bool> isEnabled)
        {
            IsEnabled = isEnabled.HasValue && isEnabled.Value;
        }

        public void IncreasePriority()
        {
            Priority++;
        }

        public void DecreasePriority()
        {
            Priority--;
        }

        public void SetPriority(int newPriority)
        {
            Priority = newPriority;
        }
    }
}
