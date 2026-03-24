# PawnShop V2 - Bao Cao Du An Full Stack

## 1. Tong quan de tai
PawnShop V2 la he thong quan ly tiem cam do duoc xay dung theo kien truc full stack, gom backend ASP.NET Core Web API va frontend Vue 3.

Muc tieu chinh:
- Quan ly khach hang, tai san, loai tai san.
- Quan ly hop dong cam co.
- Xac thuc va phan quyen nguoi dung bang JWT.
- Ho tro dong goi va trien khai nhanh bang Docker Compose.

## 2. Kien truc he thong
Du an duoc to chuc theo huong phan lop:

- `PawnShopV2.API`: Tang trinh bay (REST API, Controllers, Program).
- `PawnShopV2.Application`: Tang nghiep vu (DTOs, service interfaces, service implementations).
- `PawnShopV2.Domain`: Tang mien (Entities, Enums).
- `PawnShopV2.Infrastructure`: Tang ha tang (DbContext, seeding, services).
- `PawnShopV2Frontend`: Giao dien nguoi dung (Vue 3 + Vite + Pinia + Vue Router).

Kien truc nay giup:
- Tach biet ro trach nhiem tung tang.
- De bao tri, test va mo rong tinh nang.
- Han che coupling giua UI, nghiep vu va data access.

## 3. Cong nghe su dung
### Backend
- .NET 8 (ASP.NET Core Web API)
- Entity Framework Core
- JWT Bearer Authentication
- SQL Server hoac SQLite (cau hinh theo bien moi truong)
- Swagger/OpenAPI

### Frontend
- Vue 3
- Vite
- Pinia
- Vue Router
- Axios
- Tailwind CSS
- ECharts

### DevOps / Deployment
- Docker
- Docker Compose
- Script trien khai nhanh: `deploy.sh`, `deploy.bat`

## 4. Chuc nang chinh
- Dang nhap/dang xuat voi JWT.
- Quan ly khach hang (Customers).
- Quan ly tai san va danh muc tai san (Assets, Asset Categories).
- Quan ly hop dong cam do (Contracts).
- Seed du lieu ban dau, tao tai khoan admin mac dinh khi khoi tao he thong.

## 5. Cau hinh moi truong
Tao file `.env` tu `.env.example` tai thu muc goc, cac bien quan trong:

- `DATABASE_PROVIDER`: `Sqlite` hoac `SqlServer`.
- `DEFAULT_CONNECTION_STRING`: Chuoi ket noi CSDL.
- `JWT_SECRET_KEY`: Khoa bi mat ky token JWT.
- `JWT_ISSUER`, `JWT_AUDIENCE`: Thong tin issuer/audience JWT.
- `JWT_ACCESS_TOKEN_EXPIRATION_MINUTES`: Thoi gian song token.
- `ADMIN_DEFAULT_PASSWORD`: Mat khau tai khoan admin seed ban dau.

## 6. Huong dan chay du an
### Cach 1: Chay bang Docker Compose (khuyen nghi)
1. Tao file `.env` tu `.env.example`.
2. Chay mot trong hai script:
   - Windows: `deploy.bat`
   - Linux/macOS: `./deploy.sh`

Sau khi chay thanh cong:
- Frontend: http://localhost:5173
- Backend API: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- Tai khoan mac dinh: `admin / Admin@123` (neu duoc cau hinh seed password tuong ung)

### Cach 2: Chay thu cong tung phan
1. Backend:
   - Mo solution `PawnShopV2/PawnShopV2.sln` bang Visual Studio hoac `dotnet` CLI.
   - Chay project API (`PawnShopV2.API`).
2. Frontend:
   - Vao thu muc `PawnShopV2Frontend`.
   - Cai dat package: `npm install`
   - Chay dev server: `npm run dev`

## 7. API va co so du lieu
- API duoc dinh nghia theo mo hinh REST, cac endpoint nam trong nhom controller:
  - `AuthController`
  - `CustomersController`
  - `AssetsController`
  - `AssetCategoriesController`
  - `ContractsController`
- Backend ho tro migration/tao CSDL tu dong khi khoi dong va seed du lieu ban dau.

## 8. Danh gia va huong mo rong
### Uu diem
- Kien truc ro rang, de mo rong.
- Co xac thuc JWT va tai lieu API (Swagger).
- Trien khai nhanh, dong nhat qua Docker Compose.

### Huong phat trien tiep theo
- Bo sung test tu dong (unit test, integration test).
- Them logging, monitoring va health checks.
- Hoan thien phan quyen chi tiet theo role/permission.
- Toi uu UI/UX va bo sung bao cao thong ke nang cao.

## 9. Tac gia
- Sinh vien thuc hien: NVT-Master (cap nhat theo thong tin nhom/lop neu can).

## 10. Lien ket repository
- GitHub: https://github.com/NVT-Master/PawnShop
