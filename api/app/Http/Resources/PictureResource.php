<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class PictureResource extends JsonResource
{
    public function toArray(Request $request): array
    {
        return [
            'id'         => $this->id,
            'name'       => $this->name,
            'hash'       => $this->hash,
            'size'       => $this->size,
            'date'       => $this->date,
            'width'      => $this->width,
            'height'     => $this->height,
            'uploadedAt' => $this->created_at,
            'tags'       => $this->whenLoaded('tags', fn() =>
                $this->when($this->tags->isNotEmpty(), fn() => TagResource::collection($this->tags))
            ),
        ];
    }
}
