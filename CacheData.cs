using Rock.Data;

namespace cc.newspring.CacheBreak
{
    public class CacheData
    {
        public IEntity Entity { get; set; }
        public string Action { get; set; }
        public int Id { get; set; }
        public string EntityType { get; set; }
    }
}
