<?php

namespace App\Policies;

use App\Models\Album;
use App\Models\Complaint;
use App\Models\Picture;
use App\Models\Role;
use App\Models\User;

class AlbumPolicy
{
    /**
     * Create a new policy instance.
     */
    public function __construct()
    {
        //
    }
    public function view(User $user, Album $album): bool
    {
        dd('lol');
        if ($user->role->code === 'admin') {
            // Проверяем, есть ли жалоба на этого пользователя, помимо отклоненных
            $complaintExists = Complaint
                ::where('about_user_id', $album->user_id)
                ->where('status',null)
                ->where('album_id', $album->id)
                ->exists();

            // Если жалоба существует, разрешаем просмотр альбома
            return $complaintExists;
        }

        // По умолчанию доступ запрещен
        return false;
    }
}
