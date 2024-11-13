<?php

namespace App\Http\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class PictureResource extends JsonResource
{
    public function toArray(Request $request): array
    {
        $array = [
            'id'            => $this->id,
            'name'          => $this->name,
            'hash'          => $this->hash,
            'size'          => $this->size,
            'date'          => $this->date,
            'width'         => $this->width,
            'height'        => $this->height,
            'uploaded_at'   => $this->created_at,
        ];
        if ($this->whenLoaded('tags') && $this->tags->count())
            $array['tags'] = TagResource::collection($this->tags);
        return $array;
    }
}
