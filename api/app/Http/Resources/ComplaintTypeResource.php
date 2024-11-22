<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class ComplaintTypeResource extends JsonResource
{
    public function toArray(Request $request): array
    {
        return [
            'id'    => $this->id,
            'name'  => $this->name,
            'complaintsCount' => $this->whenCounted('complaints', fn($count) => $this->when($count, $count)),
            'complaints'      => $this->whenLoaded ('complaints', fn() =>
                $this->when($this->complaints->isNotEmpty(), fn() => PictureResource::collection($this->complaints))
            ),
        ];
    }
}
