# File Management Module

Upload and serve gym logos, profile photos, progress photos, and diet/workout attachments with gym-scoped security.

## Database (`017_FileManagementModule.sql`)

- **Files** — storage metadata (category, path, content type, size, dimensions)
- **MemberFiles** — links files to members (optional diet/workout plan IDs, notes, `TakenAt`)
- **TrainerFiles** — links files to trainers
- **Members.ProfilePhotoFileId**, **Trainers.ProfilePhotoFileId**, **Gyms.LogoFileId**

## File categories

| Category | Use |
|----------|-----|
| `GymLogo` | Gym branding |
| `MemberProfilePhoto` | Member avatar |
| `TrainerProfilePhoto` | Trainer avatar |
| `MemberProgressPhoto` | Progress gallery |
| `DietAttachment` | Diet plan files |
| `WorkoutAttachment` | Workout plan files |

## Backend

- **Storage:** `FileStorage:Provider` = `Local` (dev) or `Azure` (production)
- **Validation:** extension whitelist, size limits (`MaxFileSizeBytes`, `MaxImageSizeBytes`)
- **Images:** SixLabors.ImageSharp resize + JPEG compression
- **API:** `FilesController` at `/api/files`

### Permissions

- `VIEW_FILES` — metadata and gym logo
- `UPLOAD_FILES` — upload (members may upload own profile/progress without this)
- `DELETE_FILES` — soft delete + storage cleanup
- `MANAGE_FILES` — full access

### Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/files/upload` | Multipart upload |
| GET | `/api/files/{id}/content` | Download (anonymous for `<img>` URLs) |
| GET | `/api/files/{id}` | Metadata |
| DELETE | `/api/files/{id}` | Soft delete |
| GET | `/api/files/members/{memberId}` | Member file list |
| GET | `/api/files/trainers/{trainerId}` | Trainer file list |
| GET | `/api/files/gym/logo` | Current gym logo |

## Frontend

- `FileService`, `FileUploadComponent`, `FilePreviewComponent`
- `ProfilePhotoManagerComponent` — profile/logo management
- `MemberFilesGalleryComponent` — progress photos and attachments

### UI integration

- **Gym Admin → Branding** — gym logo
- **Super Admin → Edit Gym** — logo upload
- **Member / Trainer detail** — profile photo
- **Member detail / profile** — progress photos
- **Diet / Workout member views** — attachments

## Configuration (`appsettings.json`)

```json
"FileStorage": {
  "Provider": "Local",
  "LocalRootPath": "uploads",
  "AzureConnectionString": "",
  "AzureContainerName": "gym-files",
  "MaxFileSizeBytes": 10485760,
  "MaxImageSizeBytes": 5242880
}
```

## Deploy

1. Run `017_FileManagementModule.sql` on the database
2. Restart API (seeder adds file permissions)
3. Re-login so JWT includes new permissions

## Demo

After seeding, gym admins have full file permissions; trainers can upload/view; members can upload profile and progress photos without `UPLOAD_FILES`.
