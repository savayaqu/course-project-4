<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class UserResource extends JsonResource
{
    public function toArray(Request $request): array
    {
        $user = $request->user();
        $isAdmin = $request->attributes->get('role') === 'admin';
        $isThis  = $user?->id === $this->id;
        return [
            'id'    => $this->id,
            'name'  => $this->name,
            $this->mergeWhen($isAdmin, [
                'isBanned' => $this->is_banned,
                'complaintsAboutCount' => $this->whenCounted('complaintsAbout', fn($count) =>
                    $this->when($count, $count)
                ),
                'complaintsAboutAcceptedCount' => $this->whenCounted('complaints_about_accepted', fn($count) => $this->when($count, $count)),
                'complaintsAbout' => $this->whenLoaded ('complaintsAbout', fn() =>
                    $this->when($this->complaintsAbout->isNotEmpty(), fn() => ComplaintResource::collection($this->complaintsAbout))
                ),
            ]),
            $this->mergeWhen($isThis || $isAdmin, [
                'login' => $this->login,
                'role'  => $this->whenLoaded('role', fn() =>
                    $this->when($this->role->code !== 'user', $this->role->code)
                ),
                'complaintsFromCount'         => $this->whenCounted('complaintsFrom', fn($count) => $this->when($count, $count)),
                'complaintsFromAcceptedCount' => $this->whenCounted('complaints_from_accepted', fn($count) => $this->when($count, $count)),
                'complaintsFrom'              => $this->whenLoaded ('complaintsFrom', fn() =>
                    $this->when($this->complaintsFrom->isNotEmpty(), fn() => ComplaintResource::collection($this->complaintsFrom))
                ),
                'warningsCount' => $this->whenCounted('warnings', fn($count) => $this->when($count, $count)),
                'warnings'      => $this->whenLoaded ('warnings', fn() =>
                    $this->when($this->warnings->isNotEmpty(), fn() => WarningResource::collection($this->warnings))
                ),
                'tagsCount' => $this->whenCounted('tags', fn($count) => $this->when($count, $count)),
                'tags'      => $this->whenLoaded ('tags', fn() =>
                    $this->when($this->tags->isNotEmpty(), fn() => TagResource::collection($this->tags))
                ),
                'albumsCount' => $this->whenCounted('albums', fn($count) => $this->when($count, $count)),
                'albums'      => $this->whenLoaded ('albums', fn() =>
                    $this->when($this->albums->isNotEmpty(), fn() => AlbumResource::collection($this->albums))
                ),
                'albumsViaAccessCount' => $this->whenCounted('albumsViaAccess', fn($count) => $this->when($count, $count)),
                'albumsViaAccess'      => $this->whenLoaded ('albumsViaAccess', fn() =>
                    $this->when($this->albumsViaAccess->isNotEmpty(), fn() => UserResource::collection($this->albumsViaAccess))
                ),
                'picturesCount' => $this->whenCounted('pictures', fn($count) => $this->when($count, $count)),
                'pictures'      => $this->whenLoaded ('pictures', fn() =>
                    $this->when($this->pictures->isNotEmpty(), fn() => PictureResource::collection($this->pictures))
                ),
            ])
        ];
    }
}
