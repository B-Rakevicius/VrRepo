namespace Items
{
    public interface IWeapon
    {
        /// <summary>
        /// Start using the weapon.
        /// </summary>
        public void UseWeapon();

        /// <summary>
        /// Reload current weapon.
        /// </summary>
        /// <returns>True, if possible to reload. False otherwise.</returns>
        public bool TryReload();
    }
}
