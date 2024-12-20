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
        return [
            'id' => $this->id,
            'type' => $this->whenLoaded('type', fn() => $this->type->name),
            'status' => $this->status,
            'description' => $this->whenNotNull($this->description),
            'fromUser' => $this->whenLoaded('fromUser',
                fn() => $this->when(!$isOwner, fn() => UserResource::make($this->fromUser))
            ),
            'aboutUser' => $this->whenLoaded('aboutUser',
                fn() => UserResource::make($this->aboutUser)
            ),
            $this->when($this->whenLoaded('picture', true, false), fn() =>
                $this->mergeWhen($this->picture, fn() => [
                    'picture' => PictureResource::make($this->picture),
                    'sign' => $this->picture->album->getSign($user),
                ])
            ),
            'album' => $this->whenLoaded('album', fn() =>
                $this->when($this->album, fn() => AlbumResource::make($this->album))
            ),
        ];
    }
}
