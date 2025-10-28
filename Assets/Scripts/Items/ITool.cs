namespace Items
{
    public interface ITool
    {
        /// <summary>
        /// Start using the tool.
        /// </summary>
        public void ActivateTool();
        
        /// <summary>
        /// Deactivate the tool.
        /// </summary>
        public void DeactivateTool();
    }
}
