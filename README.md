# EVCS Backend (ASP.NET Core)

Backend mẫu cho hệ thống quản lý trạm sạc điện, được dựng theo tài liệu trong repo:
- `K14.2_SRS_Ver-1_Group10.docx`
- `Figma full`
- `figma quản lí trạm`

## 1. Kiến trúc

Solution: `EVCS.sln`

- `src/EVCS.Domain`: Entity + enum domain.
- `src/EVCS.Application`: Use case/service, DTO, business rule, thông báo lỗi tiếng Việt.
- `src/EVCS.Infrastructure`: EF Core + SQL Server + repository + seed.
- `src/EVCS.Api`: REST API, middleware xử lý lỗi, OpenAPI.

## 2. Phạm vi nghiệp vụ đã triển khai

- Quản lý `Station`: tạo, xem, cập nhật, xóa, ngừng hoạt động.
- Quản lý `Pole`: tạo, xem, cập nhật, xóa, ngừng hoạt động.
- Quản lý `ChargeType`: tạo, xem, cập nhật, xóa.
- Theo dõi trạng thái trạm qua endpoint dashboard.
- Xem lịch sử sử dụng + xuất CSV.
- Nhận và xử lý cảnh báo lỗi.

## 3. Endpoint chính

- `GET /api/stations`
- `GET /api/stations/dashboard`
- `GET /api/stations/{id}`
- `POST /api/stations`
- `PUT /api/stations/{id}`
- `PATCH /api/stations/{id}/deactivate`
- `DELETE /api/stations/{id}`

- `GET /api/poles`
- `GET /api/poles/{id}`
- `POST /api/poles`
- `PUT /api/poles/{id}`
- `PATCH /api/poles/{id}/deactivate`
- `DELETE /api/poles/{id}`

- `GET /api/charge-types`
- `GET /api/charge-types/{id}`
- `POST /api/charge-types`
- `PUT /api/charge-types/{id}`
- `DELETE /api/charge-types/{id}`

- `GET /api/usage-history`
- `GET /api/usage-history/export-csv`

- `GET /api/alerts`
- `POST /api/alerts`
- `PATCH /api/alerts/{id}/process`

## 4. Cấu hình và chạy

1. Cập nhật chuỗi kết nối trong `src/EVCS.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=EVCS_DEV_BE;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

2. Build:

```bash
dotnet build EVCS.sln /p:UseSharedCompilation=false /p:BuildInParallel=false /m:1 /nr:false
```

3. Run API:

```bash
dotnet run --project src/EVCS.Api/EVCS.Api.csproj
```

## 5. Ghi chú triển khai

- Khi app start, hệ thống áp dụng EF Core migration và seed dữ liệu mẫu nếu DB rỗng.
- Nếu không kết nối được SQL Server, app vẫn chạy và ghi cảnh báo để team cấu hình lại DB.
- Toàn bộ thông báo nghiệp vụ (`throw` message) đã để tiếng Việt có dấu để team dễ mở rộng.