<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\AlreadyExistsException;
use App\Exceptions\Api\ForbiddenException;
use App\Http\Controllers\Controller;
use App\Http\Requests\Api\Tag\TagCreateRequest;
use App\Http\Requests\Api\Tag\TagUpdateRequest;
use App\Http\Resources\TagResource;
use App\Models\Picture;
use App\Models\Tag;
use Illuminate\Support\Facades\Auth;

class TagController extends Controller
{
    public function attachToPicture($albumId, Picture $picture, Tag $tag)
    {
        if ($tag->user_id !== Auth::id())
            throw new ForbiddenException();

        $tagIds = collect([$tag->id]);
        $existingIds = $picture->tags()
            ->whereIn('tags.id', $tagIds)
            ->pluck('tags.id');

        $picture->tags()->attach($tagIds->diff($existingIds));
        return response(null, 204);
    }
    public function detachToPicture($albumId, Picture $picture, Tag $tag)
    {
        if ($tag->user_id !== Auth::id())
            throw new ForbiddenException();

        $tagIds = collect([$tag->id]);
        $existingIds = $picture->tags()
            ->whereIn('tags.id', $tagIds)
            ->pluck('tags.id');

        $picture->tags()->detach($tagIds->intersect($existingIds));
        return response(null, 204);
    }

    public function index()
    {
        $tags = Tag::where('user_id', Auth::id())->get();
        return response(TagResource::collection($tags));
    }

    public function create(TagCreateRequest $request)
    {
        $user = Auth::user();
        $existedTag = Tag
            ::where('user_id', $user->id)
            ->where('value', $request->value)
            ->first();
        if($existedTag)
            throw new AlreadyExistsException($existedTag);

        $tag = Tag::create([
           'value' => $request->value,
           'user_id' => $user->id
        ]);
        return response(['tag' => TagResource::make($tag)], 201);
    }
    public function show(Tag $tag)
    {
        if ($tag->user_id !== Auth::id())
            throw new ForbiddenException();

        $tag->load('pictures');
        return response(['tag' => TagResource::make($tag)]);
    }
    public function edit(Tag $tag, TagUpdateRequest $request)
    {
        if ($tag->user_id !== Auth::id())
            throw new ForbiddenException();

        $tag->update($request->all());
        return response(['tag' => TagResource::make($tag)])->setStatusCode(200);
    }
    public function destroy(Tag $tag)
    {
        if ($tag->user_id !== Auth::id())
            throw new ForbiddenException();

        $tag->delete();
        return response(null, 204);
    }
}
