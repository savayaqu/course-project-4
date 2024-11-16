<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class TagResource extends JsonResource
{
    public function toArray(Request $request): array
    {
        return [
            'id'    => $this->id,
            'value' => $this->value,
            'ownerId'       => $this->when($this->user_id !== $request->user()->id, fn() => $this->user_id),
            'picturesCount' => $this->whenCounted('pictures', fn($count) => $this->when($count, $count)),
            'pictures'      => $this->whenLoaded ('pictures', fn() =>
                $this->when($this->pictures->isNotEmpty(), fn() => PictureResource::collection($this->pictures))
            ),
        ];
    }
}
