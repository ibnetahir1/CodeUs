using System.ComponentModel.DataAnnotations;

namespace CodeUs.Shared.Models
{
    public class PlayInfo
    {
        public string Nickname { get; set; } = "";

        public string RoomCode { get; set; } = "";

        public bool IsHost { get; set; } = false;

    }
}
