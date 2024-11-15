<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class AlbumAccessResource extends JsonResource
{
    public function toArray(Request $request): array
    {
        return [
            'album' => AlbumResource::make($this->album),
            'user'  =>  UserResource::make($this->user),
        ];
    }
}
