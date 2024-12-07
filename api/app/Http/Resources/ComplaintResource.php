<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class ComplaintResource extends JsonResource
{
    public function toArray(Request $request): array
    {
        $user = $request->user();
        $isOwner = $this->from_user_id === $user->id;

        // Проверяем, есть ли флаг для исключения поля album
        $excludeAlbum = $request->query('exclude_album', false);

        $data = [
            'id' => $this->id,
            'type' => $this->whenLoaded('type', fn() => $this->type->name),
            'status' => $this->status,
            'description' => $this->whenNotNull($this->description),
            'fromUser' => $this->whenLoaded('fromUser', fn() => $this->when($isOwner, fn() => UserResource::make($this->fromUser))),
            'aboutUser' => $this->whenLoaded('aboutUser', fn() => UserResource::make($this->aboutUser)),
            'picture' => $this->whenLoaded('picture', fn() => PictureResource::make($this->picture)),
            'sign' => $this->whenLoaded('picture', fn() => $this->picture ? $this->picture->album->getSign($user) : null),
        ];

        // Добавляем поле album только если флаг exclude_album не установлен
        if (!$excludeAlbum) {
            $data['album'] = $this->whenLoaded('album', fn() => AlbumResource::make($this->album));
        }

        return $data;
    }

}
