<?php

namespace App\Models;

class Picture extends Model
{
    protected $fillable = [
        'name',
        'hash',
        'date',
        'size',
        'width',
        'height',
        'album_id',
    ];

    public static function getPathStatic($userId, $albumId, $name = null): string {
        return Album::getPathStatic($userId, $albumId) . "/" . $name ?? "";
    }
    public function getPath($userId): string {
        return $this->getPathStatic($userId, $this->album_id, $this->name);
    }

    public static function getPathThumbStatic($userId, $albumId, $pictureId = null, $orientation = '', $size = ''): string {
        return Album::getPathStatic($userId, $albumId) . "/thumbs/" . ($pictureId ? "$pictureId-$orientation$size.jpg" : '');
    }
    public function getPathThumb($userId, $orientation, $size): string {
        return $this->getPathThumbStatic($userId, $this->album_id, $this->id, $orientation, $size);
    }

    public function resolveRouteBinding($value, $field = null) {
        $album = request()->route('album') ?? 'NULL';
        if ($album instanceof Album) $album = $album->id;
        return $this
            ->where('album_id', $album)
            ->where($field ?? 'id', $value)
            ->firstOrFailCustom();
    }

    public function album() {
        return $this->belongsTo(Album::class);
    }
    public function tagPictures() {
        return $this->hasMany(TagPicture::class);
    }
    public function tags() {
        return $this->belongsToMany(Tag::class, 'tag_pictures');
    }
    public function complaints() {
        return $this->hasMany(Complaint::class);
    }
}
