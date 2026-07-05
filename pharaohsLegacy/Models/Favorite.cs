namespace pharaohsLegacy.Models
{
    public class Favorite
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }  
        public string Type { get; set; }        
        public int ItemId { get; set; }         
    }
}