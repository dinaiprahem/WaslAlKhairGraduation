namespace WaslAlkhair.Api.Models
{
    public enum ItemType
    {
        Lost,
        Found
    }

    public class LostItem
    {
        public Guid Id { get; set; }

        // الصورة
        public string ImagePath { get; set; }

        // Embedding لو هنستخدمه في البحث الذكي
        public string Embedding { get; set; } // JSON string or serialized float[]

        // وصف إضافي + فئة الحاجة (optional في المستقبل)
        public string Metadata { get; set; }

        public string Location { get; set; }        // المكان اللي اتفقدت أو اتلاقيت فيه
        public DateTime Date { get; set; }          // تاريخ الفقد أو العثور

        // نوع البلاغ
        public ItemType Type { get; set; }          // Lost or Found

        // حالة التسليم
        public bool IsResolved { get; set; } = false;

        // وسيلة التواصل
        public string ContactInfo { get; set; }

        // تاريخ الإنشاء
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // الربط بالـ User
        public string UserId { get; set; }          // FK لـ AspNetUsers
        public AppUser User { get; set; }   // Navigation property (optional)
    }

}
