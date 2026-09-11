# Smart Queue API

## Layihə haqqında

**Smart Queue API** müştərilərin növbəyə əlavə olunması, növbədəki mövqelərinin izlənilməsi və növbədəki növbəti müştərinin çağırılması üçün hazırlanmış RESTful Web API layihəsidir.

Sistem müştərilərin növbəyə daxil olmasını və növbədəki müştərilərin ardıcıl şəkildə çağırılmasını təmin edir.

Növbə **FIFO (First In, First Out)** prinsipi ilə işləyir. Müştərilər növbəyə daxil olduqları vaxta əsasən sıralanır.

Layihədə aşağıdakı əməliyyatlar mövcuddur:

* Müştərinin növbəyə əlavə edilməsi
* Növbədə gözləyən müştərilərin əldə edilməsi
* Müştərinin növbədəki mövqeyinin dinamik hesablanması
* Müştərinin növbədən silinməsi
* Növbədəki növbəti müştərinin çağırılması

---

## İstifadə olunan texnologiyalar

* **C#**
* **ASP.NET Core Web API**
* **Entity Framework Core**
* **PostgreSQL**
* **Npgsql**
* **REST API**
* **Swagger / OpenAPI**
* **LINQ**
* **Dependency Injection**
* **Git**

`/next` endpoint-ində PostgreSQL-in row-level locking imkanlarından istifadə etmək üçün Entity Framework Core vasitəsilə **Raw SQL** sorğusundan istifadə edilmişdir.

Əsas istifadə olunan PostgreSQL mexanizmi:

```sql
FOR UPDATE SKIP LOCKED
```

---

## API Endpoint-lər

Bütün endpoint-lər aşağıdakı base route-dan istifadə edir:

```text
/api/queue
```

### 1. Növbəyə müştəri əlavə etmək

**POST** `/api/queue`

Yeni müştərini növbəyə əlavə edir.

**Request Body:**

```json
"John"
```

Queue position database-də ayrıca saxlanılmır. Müştərinin mövqeyi lazım olduqda dinamik olaraq hesablanır.

---

### 2. Növbədə gözləyən müştəriləri əldə etmək

**GET** `/api/queue`

Hazırda `Waiting` statusunda olan bütün müştəriləri qaytarır.

Müştərilər növbəyə daxil olma sırasına əsasən qaytarılır.

---

### 3. Müştərinin növbədəki mövqeyini əldə etmək

**GET** `/api/queue/{id}`

Verilmiş `id`-yə sahib və `Waiting` statusunda olan müştərinin növbədəki mövqeyini qaytarır.

**Nümunə:**

```text
GET /api/queue/5
```

Queue position database-də ayrıca saxlanılmır. Mövqe sorğu zamanı dinamik şəkildə hesablanır.

Məsələn, növbədə:

```text
ID    CreatedAt

1     10:00
2     10:01
5     10:02
7     10:03
```

olan müştərilər varsa:

```text
Customer 1 → Position 1
Customer 2 → Position 2
Customer 5 → Position 3
Customer 7 → Position 4
```

Müştərinin mövqeyi `Waiting` statusunda olan və həmin müştəridən əvvəl növbəyə daxil olmuş müştərilərin sayı əsasında hesablanır.

---

### 4. Müştərini növbədən silmək

**DELETE** `/api/queue/{id}`

Verilmiş `id`-yə sahib müştərini növbədən silir.

**Nümunə:**

```text
DELETE /api/queue/5
```

Müştəri silindikdən sonra digər müştərilərin mövqeləri ayrıca yenilənmir. Çünki queue position hər request zamanı database-in cari vəziyyətinə əsasən hesablanır.

Məsələn:

```text
Before:

Customer 1 → Position 1
Customer 2 → Position 2
Customer 3 → Position 3
```

`Customer 2` silindikdən sonra:

```text
After:

Customer 1 → Position 1
Customer 3 → Position 2
```

---

### 5. Növbəti müştərini çağırmaq

**POST** `/api/queue/next`

Növbədə `Waiting` statusunda olan növbəti müştərini çağırır.

Müştərilər FIFO prinsipinə əsasən çağırılır.

---

## Database

Layihədə **PostgreSQL** database istifadə olunur.

Müştərilərin məlumatları və növbədəki statusları database-də saxlanılır.

Queue position ayrıca database field kimi saxlanılmır.

### Queue position-ın hesablanması

Müştərinin queue position-ı sorğu zamanı dinamik olaraq hesablanır.

Ümumi məntiq:

```text
Müştərinin Waiting statusunda olub-olmadığını yoxla
                    ↓
Waiting statusunda olan müştəriləri seç
                    ↓
Növbə sırasına əsasən əvvəlki müştəriləri hesabla
                    ↓
Müştərinin position-ını qaytar
```

Bu səbəbdən müştəri növbədən silindikdə digər müştərilərin position-larını ayrıca dəyişmək lazım deyil.

---

## `/next` problemi və həlli

`/next` endpoint-ində əsas problem **concurrency** zamanı eyni müştərinin bir neçə request tərəfindən eyni anda seçilə bilməsidir.

Məsələn, növbədə ilk müştəri `Customer 1` olduğu halda iki request eyni anda `/next` endpoint-inə göndərilərsə:

```text
Request A ──┐
            ├──> Customer 1
Request B ──┘
```

hər iki request eyni müştərini əldə edə bilər.

Bu problemin qarşısını almaq üçün **Entity Framework Core Raw SQL** və PostgreSQL-in row-level locking mexanizmindən istifadə edilmişdir.

Sorğuda:

```sql
SELECT ...
FROM ...
WHERE ...
ORDER BY ...
FOR UPDATE SKIP LOCKED
LIMIT 1
```

yanaşmasından istifadə olunur.

### `FOR UPDATE`

`FOR UPDATE` seçilmiş database sətrini transaction müddətində lock edir.

Beləliklə, həmin müştəri başqa concurrent transaction tərəfindən eyni anda seçilə bilmir.

### `SKIP LOCKED`

`SKIP LOCKED` artıq başqa transaction tərəfindən lock edilmiş sətri gözləmək əvəzinə onu ötürür və növbədəki növbəti əlçatan müştərini seçir.

Məsələn:

```text
Request A → Customer 1 🔒
Request B → Customer 2
```

Request B `Customer 1` üçün gözləmir və onu skip edərək növbəti əlçatan müştərini seçir.

Beləliklə:

```text
Request A → Customer 1
Request B → Customer 2
Request C → Customer 3
```

olması təmin edilir və eyni müştərinin iki request tərəfindən çağırılması probleminin qarşısı alınır.

---

## Layihənin necə işə salınması

### 1. Repository-ni clone edin

```bash
git clone https://github.com/NhympexX/smart-queue-api.git
```

### 2. Layihə qovluğuna keçin

```bash
cd smart-queue-api
```

### 3. PostgreSQL database yaradın

PostgreSQL-də layihə üçün database yaradın.

Məsələn:

```sql
CREATE DATABASE smart_queue;
```

### 4. Connection string-i konfiqurasiya edin

`appsettings.json` və ya `appsettings.Development.json` faylında PostgreSQL connection string-i qeyd edin.

Məsələn:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=smart_queue;Username=postgres;Password=your_password"
  }
}
```

Öz PostgreSQL istifadəçi adı, şifrə və database məlumatlarınızı qeyd edin.

### 5. Layihəni başladın

```bash
dotnet run
```

Layihə başladıqda database migration-ları avtomatik olaraq tətbiq edilir.

Migration-lar repository-də mövcuddur və tətbiq başladıqda:

```csharp
await context.Database.MigrateAsync();
```

vasitəsilə database-ə tətbiq olunur.

Buna görə ayrıca:

```bash
dotnet ef database update
```

əmrini işlətməyə ehtiyac yoxdur.

### 6. Swagger

API başladıqdan sonra endpoint-ləri Swagger vasitəsilə test etmək mümkündür:

```text
/swagger
```
