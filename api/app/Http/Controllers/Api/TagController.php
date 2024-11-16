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
use Illuminate\Http\JsonResponse;
use Illuminate\Support\Facades\Auth;

class TagController extends Controller
{
    public function attachToPicture($albumId, Picture $picture, Tag $tag): JsonResponse
    {
        if ($tag->user_id !== Auth::id())
            throw new ForbiddenException();

        $tagIds = collect([$tag->id]);
        $existingIds = $picture->tags()
            ->whereIn('tags.id', $tagIds)
            ->pluck('tags.id');

        $picture->tags()->attach($tagIds->diff($existingIds));
        return response()->json(null, 204);
    }
    public function detachToPicture($albumId, Picture $picture, Tag $tag): JsonResponse
    {
        if ($tag->user_id !== Auth::id())
            throw new ForbiddenException();

        $tagIds = collect([$tag->id]);
        $existingIds = $picture->tags()
            ->whereIn('tags.id', $tagIds)
            ->pluck('tags.id');

        $picture->tags()->detach($tagIds->intersect($existingIds));
        return response()->json(null, 204);
    }

    public function index(): JsonResponse
    {
        $tags = Tag::where('user_id', Auth::id())->get();
        return response()->json(['tags' => TagResource::collection($tags)]);
    }

    public function create(TagCreateRequest $request): JsonResponse
    {
        $value = $request->input('value');
        $user = Auth::user();
        $existedTag = Tag
            ::where('user_id', $user->id)
            ->where('value', $value)
            ->first();
        if($existedTag)
            throw new AlreadyExistsException($existedTag);

        $tag = Tag::create([
           'value' => $value,
           'user_id' => $user->id
        ]);
        return response()->json(['tag' => TagResource::make($tag)], 201);
    }
    public function show(Tag $tag): JsonResponse
    {
        if ($tag->user_id !== Auth::id())
            throw new ForbiddenException();

        $tag->load('pictures');
        return response()->json(['tag' => TagResource::make($tag)]);
    }
    public function edit(Tag $tag, TagUpdateRequest $request): JsonResponse
    {
        if ($tag->user_id !== Auth::id())
            throw new ForbiddenException();

        $tag->update($request->all());
        return response()->json(['tag' => TagResource::make($tag)]);
    }
    public function destroy(Tag $tag): JsonResponse
    {
        if ($tag->user_id !== Auth::id())
            throw new ForbiddenException();

        $tag->delete();
        return response()->json(null, 204);
    }
}
