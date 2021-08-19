﻿namespace ASC.Webhooks.Dao.Models
{
    public class WebhookEntry
    {
        public int Id { get; set; }
        public string Data { get; set; }
        public string Uri { get; set; }
        public string SecretKey { get; set; }

        public override bool Equals(object other)
        {
            var toCompareWith = other as WebhookEntry;
            if (toCompareWith == null)
                return false;
            return this.Id == toCompareWith.Id &&
                this.Data == toCompareWith.Data &&
                this.Uri == toCompareWith.Uri &&
                this.SecretKey == toCompareWith.SecretKey;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

}
