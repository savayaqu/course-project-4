<?php

namespace App\Policies;

use App\Models\Album;
use App\Models\Complaint;
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
    public function view(User $user, Album $album)
    {
        // Если пользователь является администратором
        if ($user->role() == 'admin') {
            // Проверяем, есть ли жалоба на этого пользователя
            $complaintExists = Complaint::where('about_user_id', $album->user_id)
                ->where('status', ) // Статус может быть "ожидает" или аналогичный
                ->exists();

            // Если жалоба существует, разрешаем просмотр альбома
            return $complaintExists;
        }

        // По умолчанию доступ запрещен
        return false;
    }
}
