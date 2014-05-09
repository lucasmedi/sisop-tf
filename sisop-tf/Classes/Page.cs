
namespace sisop_tf
{
    public class Page
    {
        public int Id { get; set; }
        public int Size { get; set; }
        public int FirstPosition { get; set; }
        public int Pc { get; set; }
        public PageState State { get; set; }

        public Page(int pageId, int size, int firstPosition, PageState state)
        {
            Id = pageId;
            Size = size;
            FirstPosition = firstPosition;
            State = state;
            Pc = 0;
        }
    }

    public enum PageState
    {
        Full = 0,
        Empty = 1
    }
}