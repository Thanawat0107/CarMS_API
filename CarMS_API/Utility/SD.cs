namespace CarMS_API.Utility
{
    public static class SD
    {
        public const string Role_Admin = "admin";
        public const string Role_Seller = "seller";
        public const string Role_Buyer = "buyer";

        public const string ImgPath = "/images";
        public const string ImgProductPath = "products";
        public const string ImgBrandPath = "brands";

        public const string ImgPaymentPath = "payments";

        // --- Gear Type ---
        public const string Gear_Manual = "Manual";
        public const string Gear_Automatic = "Automatic";

        // --- Engine Type ---
        public const string Engine_Gasoline = "Gasoline";
        public const string Engine_Diesel = "Diesel";
        public const string Engine_Electric = "Electric";
        public const string Engine_Hybrid = "Hybrid";

        // --- Car Type ---
        public const string CarType_FourDoorSedan = "FourDoorSedan";
        public const string CarType_PickUpTruck = "PickUpTruck";
        public const string CarType_SUV = "CarSUV";
        public const string CarType_Van = "CarVan";

        // --- Car Status ---
        public const string Status_Available = "Available"; // ว่าง/พร้อมขาย
        public const string Status_Booked = "Booked";       // จองแล้ว
        public const string Status_Sold = "Sold";           // ขายแล้ว

        // --- Booking Status ---
        public const string Reserve_Pending = "Pending";
        public const string Reserve_PendingPayment = "PendingPayment";
        public const string Reserve_Confirmed = "Confirmed";
        public const string Reserve_Expired = "Expired";
        public const string Reserve_Canceled = "Canceled";

        // --- Payment Status ---
        public const string Payment_Pending = "Pending";
        public const string Payment_Verifying = "Verifying";
        public const string Payment_Paid = "Paid";
        public const string Payment_Failed = "Failed";
        public const string Payment_Refunded = "Refunded";

        // --- Payment Method ---
        public const string PaymentMethod_QR = "QR";
        public const string PaymentMethod_PromptPay = "PromptPay";
        public const string PaymentMethod_CreditCard = "CreditCard";
        public const string PaymentMethod_BankTransfer = "BankTransfer";

        // --- Test Drive Status ---
        public const string TestDrive_Pending = "Pending";
        public const string TestDrive_Confirmed = "Confirmed";
        public const string TestDrive_Cancel = "Cancel";

        // --- Loan Status ---
        public const string Loan_Pending = "Pending";
        public const string Loan_Contacted = "Contacted";
        public const string Loan_Rejected = "Rejected";
    }
}