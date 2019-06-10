﻿namespace Kigg.Infrastructure.EF.POCO
{
    public class StoryView: Entity
    {
        public System.Guid StoryId { get; set; }
        public Story Story { get; set; }

        public System.DateTime Timestamp { get; set; }

        public string IPAddress { get; set; }
    }
}