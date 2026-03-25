หมวดการรันและคอมไพล์ (Build & Run)

dotnet build : ตรวจสอบและคอมไพล์โค้ดว่ามี Error ตรงไหนหรือไม่ (ควรทำบ่อยๆ)

dotnet run : รันโปรเจกต์เพื่อทดสอบ

dotnet watch run : (แนะนำตัวนี้ครับ!) รันโปรเจกต์แบบ Auto-reload เวลาคุณกด Save โค้ด ระบบจะรีสตาร์ทตัวเองให้ทันที ไม่ต้องมากดปิดแล้วรันใหม่ครับ

dotnet clean : ล้างไฟล์ที่ถูก Build ไว้ (ใช้เวลาที่โปรเจกต์รันแล้วมีพฤติกรรมแปลกๆ หรือ Error ค้าง)

หมวด Entity Framework Core (จัดการฐานข้อมูล)

===

(หมวดนี้คุณต้องใช้แน่นอนตอนแก้ไข Model เสร็จแล้วครับ)

dotnet ef migrations add [Name] : สร้างไฟล์ Migration ใหม่เพื่อเตรียมอัปเดตฐานข้อมูล (เช่น dotnet ef migrations add UpdateCarModel)

dotnet ef database update : นำไฟล์ Migration ไปสร้างหรืออัปเดตตารางในฐานข้อมูล SQL จริงๆ

dotnet ef migrations remove : ลบ Migration ตัวล่าสุดทิ้ง (ใช้กรณีที่กด add ไปแล้วเพิ่งนึกได้ว่าลืมเพิ่มฟิลด์)

dotnet ef database drop : ลบฐานข้อมูลทิ้งทั้งก้อน (ระวัง! ใช้เฉพาะตอนเทสต์แล้วอยากล้างไพ่ใหม่เท่านั้นครับ)