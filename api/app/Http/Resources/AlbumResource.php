<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class AlbumResource extends JsonResource
{
    public function toArray(Request $request): array
    {
        $user = $request->user();
        $isOwner = $this->user_id === $user->id;
        $isAdmin = $request->attributes->get('role') === 'admin';
        //Убрать из ответа массив жалоб при просмотре конкретного альбома
        if ($request->routeIs('albums.show')) {
            unset($this['complaints']);
        }
        return [
            'id'    => $this->id,
            'name'  => $this->name,
            'owner' => $this->whenLoaded('user',
                fn() => $this->when(!$isOwner, fn() => UserResource::make($this->user))
            ),
            $this->mergeWhen($isOwner || $isAdmin, [
                'path'      => $this->whenNotNull($this->path),
                'createdAt' => $this->created_at,
                'grantAccessesCount' => $this->whenCounted('usersViaAccess', fn($count) => $this->when($count, $count)),
                'grantAccesses'      => $this->whenLoaded ('usersViaAccess', fn() =>
                    $this->when($this->usersViaAccess->isNotEmpty(), fn() => UserResource::collection($this->usersViaAccess))
                ),
                'invitationsCount'   => $this->whenCounted('invitations', fn($count) => $this->when($count, $count)),
                'invitations'        => $this->whenLoaded ('invitations', fn() =>
                    $this->when($this->invitations->isNotEmpty(), fn() => InvitationResource::collection($this->invitations))
                ),
            ]),
            'picturesCount' => $this->whenCounted('pictures', fn($count) => $this->when($count, $count)),
            'picturesInfo'  => $this->whenLoaded ('pictures', fn() =>
                $this->when($this->pictures->isNotEmpty(), fn() => [
                    'sign' => $this->getSign($user),
                    'ids' => $this->pictures->pluck('id'),
            ])),

            'complaintsCount' => $this->whenCounted('complaints', fn($count) => $this->when($count, $count)),
            'complaints' => $this->whenLoaded ('complaints', fn() =>
                    $this->when($this->complaints->isNotEmpty(), fn() => ComplaintResource::collection($this->complaints))),
        ];
    }
}
