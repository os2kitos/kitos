namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Represents that an interface exposes some data.
    /// The interface will have a datarow for each piece of data that the
    /// interface exposes.
    /// </summary>
    public class DataRow : Entity, ISystemModule
    {
        public DataRow()
        {
        }

        public int ItInterfaceId { get; set; }
        /// <summary>
        /// The interface which exposes the data
        /// </summary>
        public virtual ItInterface ItInterface { get; set; }

        public int? DataTypeId { get; set; }
        /// <summary>
        /// The type of the data
        /// </summary>
        public virtual DataType DataType { get; set; }

        /// <summary>
        /// Description/name of the data
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Determines whether a user has write access to this instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///   <c>true</c> if user has write access, otherwise <c>false</c>.
        /// </returns>
        public override bool HasUserWriteAccess(User user)
        {
            if (ItInterface != null && ItInterface.HasUserWriteAccess(user))
                return true;

            return base.HasUserWriteAccess(user);
        }
    }
}
